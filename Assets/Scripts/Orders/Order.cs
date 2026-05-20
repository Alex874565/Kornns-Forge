using Unity.Netcode;
using UnityEngine;

public class Order : NetworkBehaviour, IThrowable, IPlayerInteractable
{
    [Header("Setup")]
    [SerializeField] private OrderData orderData;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D coll;

    private Transform followTarget;
    private PlayerStatusController playerParent;

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

    // ---------------- DATA ----------------

    public OrderData GetOrderData()
    {
        return orderData;
    }

    public void SetOrderData(OrderData data)
    {
        orderData = data;
    }

    // ---------------- HOLDING ----------------

    public void SetOrderParent(PlayerStatusController parent)
    {
        if (!IsServer) return;
        if (parent == null) return;

        if (!parent.TrySetOrder(this))
        {
            Debug.LogError("Player already holding something");
            return;
        }

        playerParent = parent;

        NetworkObject parentNetworkObject = parent.GetComponent<NetworkObject>();

        if (parentNetworkObject == null)
        {
            Debug.LogError("Parent has no NetworkObject.");
            return;
        }

        SetHeldState(parentNetworkObject.NetworkObjectId);
        SetHeldStateClientRpc(parentNetworkObject.NetworkObjectId);
    }

    private void SetHeldState(ulong parentId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                parentId,
                out NetworkObject parentObj))
            return;

        PlayerStatusController parent =
            parentObj.GetComponent<PlayerStatusController>();

        if (parent == null) return;

        playerParent = parent;
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
    private void SetHeldStateClientRpc(ulong parentId)
    {
        SetHeldState(parentId);
    }

    private void ClearHeldState()
    {
        if (IsServer && playerParent != null)
            playerParent.ClearOrder();

        playerParent = null;
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

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
        }

        if (coll != null)
            coll.enabled = true;

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

        SetOrderParent(playerStatusController);
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
            SetOrderParent(player);
    }

    public bool CanInteract(PlayerStatusController playerStatusController)
    {
        return playerStatusController != null &&
               !playerStatusController.IsHoldingSomething();
    }

    // ---------------- SPAWNING / DESTROYING ----------------

    public static Order SpawnOrder(
        OrderData orderData,
        PlayerStatusController parent,
        Transform orderPrefab)
    {
        if (orderData == null || parent == null || orderPrefab == null)
            return null;

        Transform orderTransform = Instantiate(orderPrefab);

        Order order = orderTransform.GetComponent<Order>();
        if (order == null)
        {
            Destroy(orderTransform.gameObject);
            return null;
        }

        order.SetOrderData(orderData);

        NetworkObject netObj = order.GetComponent<NetworkObject>();

        if (netObj == null)
        {
            Destroy(orderTransform.gameObject);
            return null;
        }

        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsServer)
        {
            netObj.Spawn(true);
        }

        order.SetOrderParent(parent);

        return order;
    }

    public void DestroySelf()
    {
        if (!IsServer) return;

        ClearHeldState();
        ClearHeldStateClientRpc();

        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn(true);
        else
            Destroy(gameObject);
    }

    // ---------------- VISUAL ----------------

    public void Highlight() { }

    public void UnHighlight() { }
}