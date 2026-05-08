using UnityEngine;

[CreateAssetMenu(menuName = "Orders/Order Database")]
public class OrderDatabase : ScriptableObject
{
    public OrderData[] orders;

    public int GetIndex(OrderData order)
    {
        return System.Array.IndexOf(orders, order);
    }

    public OrderData GetOrder(int index)
    {
        if (index < 0 || index >= orders.Length)
            return null;

        return orders[index];
    }
}