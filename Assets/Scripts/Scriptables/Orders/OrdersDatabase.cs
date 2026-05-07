using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Orders/Orders Database")]
public class OrdersDatabase : ScriptableObject
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

    public OrderData GetCraftableOrder(List<Ingredient> ingredients)
    {
        foreach(OrderData order in orders)
        {
            if (order.CanCraft(ingredients))
            {
                return order;
            }
        }
        return null;
    }
}