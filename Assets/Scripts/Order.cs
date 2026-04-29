using UnityEngine;

public class Order : MonoBehaviour
{
    [SerializeField] private OrderData orderData;

    private PlayerStatusController playerParent;

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
}