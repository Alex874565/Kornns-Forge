using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public List<OrderProgress> activeOrders = new();

    public System.Action OnOrdersUpdated;
    private IOrderFactory orderFactory;

    [SerializeField] private int maxActiveOrders = 7;

    private Queue<(OrderData order, float timer, int points)> queuedOrders = new();

    private void Awake()
    {
        orderFactory = new OrderFactory();
    }

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
                TryFillOrderSlot();
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

        TryFillOrderSlot();

        OnOrdersUpdated?.Invoke();
    }

    public void AddOrder(OrderData order, float timer, int points)
    {
        if (activeOrders.Count >= maxActiveOrders)
        {
            queuedOrders.Enqueue((order, timer, points));
            Debug.Log($"Queued order: {order.orderName}");
            return;
        }

        CreateAndAddOrder(order, timer, points);
        OnOrdersUpdated?.Invoke();
    }

    private void CreateAndAddOrder(OrderData order, float timer, int points)
    {
        OrderProgress newOrder = orderFactory.CreateOrder(order, timer, points);

        if (newOrder == null) return;

        activeOrders.Add(newOrder);

        Debug.Log($"Added order: {order.orderName}");
    }

    private void TryFillOrderSlot()
    {
        if (queuedOrders.Count == 0) return;

        if (activeOrders.Count >= maxActiveOrders)
            return;

        var queued = queuedOrders.Dequeue();

        CreateAndAddOrder(queued.order, queued.timer, queued.points);
    }
    
    public IReadOnlyList<OrderProgress> GetActiveOrders()
    {
        return activeOrders;
    }

    public void CompleteOrder(OrderProgress orderProgress)
    {
        if (orderProgress == null) return;

        activeOrders.Remove(orderProgress);

        TryFillOrderSlot();

        OnOrdersUpdated?.Invoke();
    }
}