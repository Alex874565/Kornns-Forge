using Unity.Netcode;
using UnityEngine;

public class Order : NetworkBehaviour, IThrowable, IPlayerInteractable
{
    [SerializeField] private OrderData orderData;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D coll;
    private Transform followTarget;

    private PlayerStatusController playerParent;

    private void Awake()
    {
        if(rb == null) rb = GetComponent<Rigidbody2D>();
        if (coll == null) coll = GetComponent<Collider2D>();
    }
    
    private void LateUpdate()
    {
        if (followTarget == null) return;

        transform.position = followTarget.position;
        transform.rotation = followTarget.rotation;
    }
    
    public OrderData GetOrderData()
    {
        return orderData;
    }

    public void SetOrderData(OrderData data)
    {
        orderData = data;
    }

    public void SetOrderParent(PlayerStatusController parent)
    {
        if (parent == null) return;

        if (playerParent != null)
            playerParent.ClearOrder();

        if (!parent.TrySetOrder(this))
        {
            Debug.LogError("Player already holding something");
            return;
        }

        playerParent = parent;
        followTarget = parent.GetIngredientFollowTransform();

        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject parentNetworkObject =
                followTarget.GetComponentInParent<NetworkObject>();

            if (parentNetworkObject != null)
                NetworkObject.TrySetParent(parentNetworkObject, true);
        }

        transform.position = followTarget.position;
        transform.rotation = followTarget.rotation;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void DestroySelf()
    {
        if (playerParent != null)
        {
            playerParent.ClearOrder();
            playerParent = null;
        }

        followTarget = null;

        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn(true);
        else
            Destroy(gameObject);
    }
    
    public static Order SpawnOrder(OrderData orderData, PlayerStatusController parent, Transform orderPrefab)
    {
        Transform orderTransform = Instantiate(orderPrefab);

        Order order = orderTransform.GetComponent<Order>();
        order.SetOrderData(orderData);

        NetworkObject netObj = order.GetComponent<NetworkObject>();

        if (parent.IsServer)
        {
            netObj.Spawn(true);
        }

        order.SetOrderParent(parent);

        return order;
    }

    public void ThrowSelf(Vector2 direction, float force, float angle)
    {
        if (!IsServer) return;
        
        if (direction == Vector2.zero) return;

        if (playerParent != null)
        {
            playerParent.ClearOrder();
            playerParent = null;
        }

        followTarget = null;

        if (IsServer && NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.TryRemoveParent();
        else
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

        rb.AddForce(angledDirection * force, ForceMode2D.Impulse);
    }
    
    #region Interaction
    
    public void Highlight()
    {
        return;
    }

    public void UnHighlight()
    {
        return;
    }

    public void Interact(PlayerStatusController playerStatusController)
    {
        Debug.Log($"Interacting {gameObject.name}");
        if (CanInteract(playerStatusController))
        {
            SetOrderParent(playerStatusController);
        }
    }

    public bool CanInteract(PlayerStatusController playerStatusController)
    {
        return playerStatusController != null &&
               !playerStatusController.IsHoldingSomething() &&
               playerParent == null;
    }
    
    #endregion
}