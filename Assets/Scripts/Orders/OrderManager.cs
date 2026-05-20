using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OrderManager : NetworkBehaviour
{
    public List<OrderProgress> activeOrders = new();

    public System.Action OnOrdersUpdated;
    private IOrderFactory orderFactory;

    [SerializeField] private int maxActiveOrders = 7;

    [Header("Sync")]
    [SerializeField] private OrdersDatabase ordersDatabase;

    private Queue<(OrderData order, float timer, int points)> queuedOrders = new();

    private void Awake()
    {
        orderFactory = new OrderFactory();
    }

    private void Update()
    {
        if (!IsServer) return;

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
            SyncOrdersClientRpc();
    }

    public void AddItem(IngredientSO ingredient)
    {
        if (!IsServer) return;
        if (ingredient == null) return;

        foreach (OrderProgress progress in activeOrders)
        {
            if (progress.TryAddItem(ingredient))
            {
                SyncOrdersClientRpc();
                return;
            }
        }

        Debug.Log($"No active order needs {ingredient.objectName}");
    }

    public void TryCraft()
    {
        if (!IsServer) return;

        foreach (OrderProgress progress in activeOrders)
        {
            if (progress.crafted) continue;

            if (progress.IsComplete())
            {
                progress.MarkCrafted();

                Debug.Log($"Crafted {progress.order.orderName}!");
                SyncOrdersClientRpc();
                return;
            }
        }

        Debug.Log("No complete order to craft.");
    }

    public void AddOrder(OrderData order, float timer, int points)
    {
        if (!IsServer) return;
        if (order == null) return;

        if (activeOrders.Count >= maxActiveOrders)
        {
            queuedOrders.Enqueue((order, timer, points));
            Debug.Log($"Queued order: {order.orderName}");
            return;
        }

        CreateAndAddOrder(order, timer, points);
        SyncOrdersClientRpc();
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
        if (activeOrders.Count >= maxActiveOrders) return;

        var queued = queuedOrders.Dequeue();
        CreateAndAddOrder(queued.order, queued.timer, queued.points);
    }

    public IReadOnlyList<OrderProgress> GetActiveOrders()
    {
        return activeOrders;
    }

    public void CompleteOrder(OrderProgress orderProgress)
    {
        if (!IsServer) return;
        if (orderProgress == null) return;

        activeOrders.Remove(orderProgress);

        TryFillOrderSlot();

        SyncOrdersClientRpc();
    }

    [ClientRpc]
    private void SyncOrdersClientRpc()
    {
        int count = activeOrders.Count;

        int[] orderIndexes = new int[count];
        float[] timers = new float[count];
        int[] points = new int[count];
        bool[] crafted = new bool[count];

        for (int i = 0; i < count; i++)
        {
            OrderProgress progress = activeOrders[i];

            orderIndexes[i] = GetOrderIndex(progress.order);
            timers[i] = progress.timeRemaining;
            points[i] = progress.points;
            crafted[i] = progress.crafted;
        }

        ReceiveOrdersClientRpc(orderIndexes, timers, points, crafted);
    }

    [ClientRpc]
    private void ReceiveOrdersClientRpc(int[] orderIndexes, float[] timers, int[] points, bool[] crafted)
    {
        activeOrders.Clear();

        for (int i = 0; i < orderIndexes.Length; i++)
        {
            int orderIndex = orderIndexes[i];

            OrderData orderData = ordersDatabase.GetOrderByIndex(orderIndex);

            OrderProgress progress = orderFactory.CreateOrder(
                orderData,
                timers[i],
                points[i]
            );

            progress.crafted = crafted[i];

            activeOrders.Add(progress);
        }

        OnOrdersUpdated?.Invoke();
    }

    private int GetOrderIndex(OrderData order)
    {
        int index = ordersDatabase.GetOrderIndex(order);

        if (index == -1)
            Debug.LogError($"Order not found in OrdersDatabase: {order.name}");

        return index;
    }
}