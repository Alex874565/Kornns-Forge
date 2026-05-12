using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Orders/Orders Database")]
public class OrdersDatabase : ScriptableObject
{
    public OrderData[] orders;

    // ---------------- ORDERS ----------------

    public int GetOrderIndex(OrderData order)
    {
        if (order == null)
            return -1;

        return System.Array.IndexOf(orders, order);
    }

    public OrderData GetOrderByIndex(int index)
    {
        if (index < 0 || index >= orders.Length)
            return null;

        return orders[index];
    }

    // ---------------- CRAFTING ----------------

    public OrderData GetCraftableOrder(List<IngredientSO> ingredients)
    {
        foreach (OrderData order in orders)
        {
            if (order != null && order.CanCraft(ingredients))
                return order;
        }

        return null;
    }
}