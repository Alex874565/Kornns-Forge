using Unity.Netcode;
using UnityEngine;

public class OrderTimelineReceiver : NetworkBehaviour
{
    [SerializeField] private OrderDatabase orderDatabase;
    [SerializeField] private OrderManager orderManager;

    public void SpawnOrder(OrderData order, float timer, int points)
    {
        if (!IsServer)
            return;

        if (order == null)
        {
            Debug.LogError("Timeline tried to spawn a null order.");
            return;
        }

        int index = orderDatabase.GetIndex(order);

        if (index == -1)
        {
            Debug.LogError($"Order {order.name} is not in the Order Database.");
            return;
        }

        SpawnOrderClientRpc(index, timer, points);
    }

    [ClientRpc]
    private void SpawnOrderClientRpc(int orderIndex, float timer, int points)
    {
        OrderData order = orderDatabase.GetOrder(orderIndex);

        if (order == null)
        {
            Debug.LogError($"No order found at index {orderIndex}.");
            return;
        }

        orderManager.AddOrder(order, timer, points);
    }
}