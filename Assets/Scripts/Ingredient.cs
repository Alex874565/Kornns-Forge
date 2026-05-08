using Unity.Netcode;
using UnityEngine;

public class Ingredient : NetworkBehaviour, IThrowable, IPlayerInteractable
{
    [SerializeField] private IngredientSO ingredientSO;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D coll;

    private Transform followTarget;
    
    private IIngredientParent ingredientParent;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (coll == null) coll = GetComponent<Collider2D>();
    }

    private void LateUpdate()
    {
        if (followTarget == null) return;

        transform.position = followTarget.position;
        transform.rotation = followTarget.rotation;
    }
    
    public IngredientSO GetIngredientSO()
    {
        return ingredientSO;
    }

    public void SetIngredientParent(IIngredientParent parent)
    {
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

        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject parentNetworkObject =
                followTransform.GetComponentInParent<NetworkObject>();

            if (parentNetworkObject == null)
            {
                Debug.LogError("Parent has no NetworkObject in parents.");
                return;
            }

            NetworkObject.TrySetParent(parentNetworkObject, true);
        }
        else
        {
            transform.SetParent(followTransform);
        }

        followTarget = parent.GetIngredientFollowTransform();
        transform.position = followTransform.position;
        transform.rotation = followTransform.rotation;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Debug.Log($"Parenting ingredient to: {followTransform.name}");
    }

    public IIngredientParent GetIngredientParent()
    {
        return ingredientParent;
    }

    public void DestroySelf()
    {
        if (ingredientParent != null)
        {
            ingredientParent.ClearIngredient();
            ingredientParent = null;
        }

        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }

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

    public void ThrowSelf(Vector2 direction, float force, float angle)
    {
        if (direction == Vector2.zero) return;

        if (ingredientParent != null)
        {
            ingredientParent.ClearIngredient();
            ingredientParent = null;
        }

        transform.SetParent(null);

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        float xSign = Mathf.Sign(direction.x);

        Vector2 angledDirection = new Vector2(
            xSign * Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ).normalized;

        followTarget = null;

        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.TryRemoveParent();
        else
            transform.SetParent(null);
        
        rb.AddForce(angledDirection * force, ForceMode2D.Impulse);
    }

    public void Highlight() { }

    public void UnHighlight() { }

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

        if (CanInteract(player))
            SetIngredientParent(player);
    }

    public bool CanInteract(PlayerStatusController playerStatusController)
    {
        return playerStatusController != null &&
               !playerStatusController.IsHoldingSomething() &&
               ingredientParent == null;
    }
}