using UnityEngine;

public class Order : MonoBehaviour, IThrowable
{
    [SerializeField] private OrderData orderData;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D coll;

    private PlayerStatusController playerParent;

    private void Awake()
    {
        if(rb == null) rb = GetComponent<Rigidbody2D>();
        if (coll == null) coll = GetComponent<Collider2D>();
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

        transform.SetParent(parent.GetIngredientFollowTransform());
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    public void DestroySelf()
    {
        if (playerParent != null)
            playerParent.ClearOrder();

        Destroy(gameObject);
    }

    public static Order SpawnOrder(OrderData orderData, PlayerStatusController parent, Transform orderPrefab)
    {
        Transform orderTransform = Instantiate(orderPrefab);

        Order order = orderTransform.GetComponent<Order>();
        order.SetOrderData(orderData);
        order.SetOrderParent(parent);

        return order;
    }

    public void ThrowSelf(Vector2 direction, float force, float angle)
    {
        if (direction == Vector2.zero) return;

        if (playerParent != null)
        {
            playerParent.ClearOrder();
            playerParent = null;
        }

        transform.SetParent(null);

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 🔥 Rotate direction by angle
        Vector2 angledDirection = Quaternion.Euler(0, 0, angle) * direction.normalized;

        rb.AddForce(angledDirection * force, ForceMode2D.Impulse);
    }
}