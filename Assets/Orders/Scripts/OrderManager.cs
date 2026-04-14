using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class OrderManager : MonoBehaviour
{
    public List<OrderProgress> activeOrders = new List<OrderProgress>();

    /* Event to which the UI subscribes to */
    public System.Action OnOrdersUpdated;

    /* This function can be called after the furnance finishes to melt one metal for example */
    public void AddItem(ItemType item, MaterialType material, MaterialProcess state)
    {
        CollectedItem newItem = new CollectedItem
        {
            itemType = item,
            materialType = material,
            state = state
        };

        /* Find first order that needs this item */
        foreach (var progress in activeOrders)
        {
            if (progress.NeedsItem(item, material, state))
            {
                progress.collectedItems.Add(newItem);
                OnOrdersUpdated?.Invoke();
                return;
            }
        }
    }

    private void Update()
    {
        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            var order = activeOrders[i];
            order.UpdateTimer(Time.deltaTime);

            if (order.IsExpired())
            {
                Debug.Log($"Order expired: {order.order.orderName}");
                activeOrders.RemoveAt(i);
                OnOrdersUpdated?.Invoke();
            }
        }
    }

    public void TryCraft()
    {
        foreach (var progress in activeOrders)
        {
            if (progress.crafted) continue;

            if (progress.IsComplete())
            {
                progress.crafted = true;
                Debug.Log($"Crafted {progress.order.orderName}!");
                OnOrdersUpdated?.Invoke();
                return;
            }
        }
        Debug.Log("No complete order to craft.");
    }
    
    /* Delivers the first crafter order from the list */
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
    }

    private void Deliver(OrderProgress progress)
    {
        Debug.Log($"Delivered {progress.order.orderName}! Reward: {progress.order.reward}");
        activeOrders.Remove(progress);
        OnOrdersUpdated?.Invoke();
    }

    public void AddOrder(OrderData order)
    {
        activeOrders.Add(new OrderProgress(order, 60f));
        OnOrdersUpdated?.Invoke();
    }
}