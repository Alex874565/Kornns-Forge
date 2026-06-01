using DG.Tweening;using Unity.Netcode;using Unity.Netcode.Components;using UnityEngine;

public class Ingredient : NetworkBehaviour, IThrowable, IPlayerInteractable
{
    [Header("Setup")] [SerializeField] private IngredientSO ingredientSO;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D coll;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private IIngredientParent ingredientParent;
    private Transform followTarget;
    private NetworkTransform networkTransform;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (coll == null) coll = GetComponent<Collider2D>();
        if (networkTransform == null) networkTransform = GetComponent<NetworkTransform>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        spriteRenderer.transform.localScale = Vector3.zero;
    }

    public override void OnNetworkSpawn()
    {
        if (networkTransform == null)
            networkTransform = GetComponent<NetworkTransform>();

        spriteRenderer.transform.DOScale(Vector3.one, 0.3f).From(Vector3.zero).SetEase(Ease.OutBack, 3);
    }

    private void LateUpdate()
    {
        if (followTarget == null)
            return;

        transform.position = followTarget.position;
        transform.rotation = followTarget.rotation;
    }

// ---------------- GETTERS ----------------

    public IngredientSO GetIngredientSO()
    {
        return ingredientSO;
    }

    public IIngredientParent GetIngredientParent()
    {
        return ingredientParent;
    }

// ---------------- PARENTING / HOLDING ----------------

    public void SetIngredientParent(IIngredientParent parent)
    {
        if (!IsServer) return;
        if (parent == null) return;

        if (ingredientParent != null)
            ingredientParent.ClearIngredient();

        if (parent.HasIngredient())
        {
            Debug.LogError("IIngredientParent already has an ingredient");
            return;
        }

        ingredientParent = parent;
        parent.SetIngredient(this);

        Transform followTransform = parent.GetIngredientFollowTransform();

        NetworkObject parentNetworkObject =
            followTransform.GetComponentInParent<NetworkObject>();

        if (parentNetworkObject == null)
        {
            Debug.LogError("Parent has no NetworkObject in parents.");
            return;
        }

        SetHeldState(parentNetworkObject.NetworkObjectId);
        SetHeldStateClientRpc(parentNetworkObject.NetworkObjectId);

        Debug.Log($"Parenting ingredient to: {followTransform.name}");
    }

    private void SetHeldState(ulong parentNetworkObjectId)
    {
        SetNetworkTransformEnabled(false);

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                parentNetworkObjectId,
                out NetworkObject parentObj))
            return;

        IIngredientParent parent = parentObj.GetComponent<IIngredientParent>();
        if (parent == null) return;

        ingredientParent = parent;
        followTarget = parent.GetIngredientFollowTransform();

        transform.SetParent(null);
        transform.position = followTarget.position;
        transform.rotation = followTarget.rotation;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (coll != null)
            coll.enabled = false;
    }

    [ClientRpc]
    private void SetHeldStateClientRpc(ulong parentNetworkObjectId)
    {
        SetHeldState(parentNetworkObjectId);
    }

    private void ClearHeldState()
    {
        if (ingredientParent != null)
        {
            ingredientParent.ClearIngredient();
            ingredientParent = null;
        }

        followTarget = null;
        transform.SetParent(null);

        if (coll != null)
            coll.enabled = true;
    }

    [ClientRpc]
    private void ClearHeldStateClientRpc()
    {
        ClearHeldState();
    }

// ---------------- THROWING ----------------

    public void ThrowSelf(Vector2 direction, float force, float angle)
    {
        if (!IsServer) return;
        if (direction == Vector2.zero) return;

        ClearHeldState();
        ClearHeldStateClientRpc();

        SetNetworkTransformEnabled(true);
        SetNetworkTransformEnabledClientRpc(true);

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
        }

        float xSign = direction.x >= 0 ? 1f : -1f;

        Vector2 angledDirection = new Vector2(
            xSign * Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ).normalized;

        rb.AddForce(angledDirection * force, ForceMode2D.Impulse);
    }

// ---------------- INTERACTION ----------------

    public void Interact(PlayerStatusController playerStatusController)
    {
        Debug.Log($"Interacting {gameObject.name}");

        if (!CanInteract(playerStatusController))
            return;

        if (!IsServer)
        {
            PickUpServerRpc(playerStatusController.NetworkObjectId);
            return;
        }

        SetIngredientParent(playerStatusController);
        playerStatusController.SetIngredient(this);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PickUpServerRpc(ulong playerId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                playerId,
                out NetworkObject playerObject))
            return;

        PlayerStatusController player =
            playerObject.GetComponent<PlayerStatusController>();

        if (player == null) return;

        if (CanInteract(player))
        {
            SetIngredientParent(player);
            player.SetIngredient(this);
        }
    }

    public bool CanInteract(PlayerStatusController playerStatusController)
    {
        return playerStatusController != null &&
               !playerStatusController.IsHoldingSomething();
    }

// ---------------- SPAWNING / DESTROYING ----------------

    public static Ingredient SpawnIngredient(
        IngredientSO ingredientSO,
        IIngredientParent ingredientParent)
    {
        if (ingredientSO == null)
        {
            Debug.LogError("SpawnIngredient failed: ingredientSO is null.");
            return null;
        }

        if (ingredientSO.prefab == null)
        {
            Debug.LogError($"SpawnIngredient failed: {ingredientSO.name} has no prefab.");
            return null;
        }

        if (ingredientParent == null)
        {
            Debug.LogError("SpawnIngredient failed: ingredientParent is null.");
            return null;
        }

        Transform ingredientTransform = Instantiate(ingredientSO.prefab);
        Ingredient ingredient = ingredientTransform.GetComponent<Ingredient>();

        if (ingredient == null)
        {
            Debug.LogError("SpawnIngredient failed: prefab has no Ingredient component.");
            Destroy(ingredientTransform.gameObject);
            return null;
        }

        NetworkObject netObj = ingredient.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Debug.LogError("SpawnIngredient failed: prefab has no NetworkObject.");
            Destroy(ingredientTransform.gameObject);
            return null;
        }

        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsServer)
        {
            netObj.Spawn(true);
        }

        ingredient.SetIngredientParent(ingredientParent);

        return ingredient;
    }

    public void DestroySelf()
    {
        if (IsServer)
        {
            ClearHeldState();
            ClearHeldStateClientRpc();

            if (NetworkObject != null && NetworkObject.IsSpawned)
                NetworkObject.Despawn(true);
            else
                Destroy(gameObject);
        }
    }

// ---------------- NETWORK TRANSFORM ----------------

    private void SetNetworkTransformEnabled(bool enabled)
    {
        if (networkTransform == null)
            networkTransform = GetComponent<NetworkTransform>();

        if (networkTransform != null)
            networkTransform.enabled = enabled;
    }

    [ClientRpc]
    private void SetNetworkTransformEnabledClientRpc(bool enabled)
    {
        SetNetworkTransformEnabled(enabled);
    }

// ---------------- VISUAL ----------------

    public void Highlight()
    {
    }

    public void UnHighlight()
    {
    }
}