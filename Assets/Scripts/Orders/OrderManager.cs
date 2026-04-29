using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public List<OrderProgress> activeOrders = new();

    public System.Action OnOrdersUpdated;

    public void AddItem(IngredientSO ingredient)
    {
        if (ingredient == null) return;

        foreach (OrderProgress progress in activeOrders)
        {
            if (progress.TryAddItem(ingredient))
            {
                OnOrdersUpdated?.Invoke();
                return;
            }
        }

        Debug.Log($"No active order needs {ingredient.objectName}");
    }

    private void Update()
    {
        bool changed = false;

        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            OrderProgress order = activeOrders[i];

            order.UpdateTimer(Time.deltaTime);

            if (order.IsExpired())
            {
                Debug.Log($"Order expired: {order.order.orderName}");
                activeOrders.RemoveAt(i);
                changed = true;
            }
        }

        if (changed)
            OnOrdersUpdated?.Invoke();
    }

    public void TryCraft()
    {
        foreach (OrderProgress progress in activeOrders)
        {
            if (progress.crafted) continue;

            if (progress.IsComplete())
            {
                progress.MarkCrafted();

                Debug.Log($"Crafted {progress.order.orderName}!");
                OnOrdersUpdated?.Invoke();
                return;
            }
        }

        Debug.Log("No complete order to craft.");
    }

    public void TryDeliver()
    {
        for (int i = 0; i < activeOrders.Count; i++)
        {
            if (activeOrders[i].crafted)
            {
                Deliver(activeOrders[i]);
                return;
            }
        }

        Debug.Log("No crafted order to deliver.");
    }

    private void Deliver(OrderProgress progress)
    {
        if (progress == null) return;

        Debug.Log($"Delivered {progress.order.orderName}! Reward: {progress.order.reward}");

        activeOrders.Remove(progress);
        OnOrdersUpdated?.Invoke();
    }

    public void AddOrder(OrderData order)
    {
        if (order == null) return;

        activeOrders.Add(new OrderProgress(order, 60f));
        OnOrdersUpdated?.Invoke();
    }
}