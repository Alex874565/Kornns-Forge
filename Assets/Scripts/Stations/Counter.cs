using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Counter : BaseStation
{
    [SerializeField] private OrderManager orderManager;

    public override bool CanInteract(PlayerStatusController player)
    {
        if (player == null) return false;
        if (!player.HasOrder()) return false;
        if (orderManager == null) return false;

        Order heldOrder = player.GetOrder();
        if (heldOrder == null) return false;

        OrderData heldOrderData = heldOrder.GetOrderData();

        return orderManager.GetActiveOrders().Any(activeOrder =>
            !activeOrder.crafted &&
            !activeOrder.IsExpired() &&
            activeOrder.order == heldOrderData
        );
    }

    public override void Interact(PlayerStatusController player)
    {
        Debug.Log("Counter.Interact()");

        if (!IsServer)
        {
            SubmitOrderServerRpc(player.NetworkObjectId);
            return;
        }

        SubmitOrder(player);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitOrderServerRpc(ulong playerNetworkObjectId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                playerNetworkObjectId,
                out NetworkObject playerNetworkObject))
            return;

        PlayerStatusController player =
            playerNetworkObject.GetComponent<PlayerStatusController>();

        SubmitOrder(player);
    }

    private void SubmitOrder(PlayerStatusController player)
    {
        if (player == null) return;
        if (!player.HasOrder()) return;

        Order heldOrder = player.GetOrder();
        if (heldOrder == null) return;

        OrderData heldOrderData = heldOrder.GetOrderData();

        OrderProgress targetOrder = orderManager.GetActiveOrders()
            .Where(activeOrder =>
                !activeOrder.crafted &&
                !activeOrder.IsExpired() &&
                activeOrder.order == heldOrderData
            )
            .OrderBy(activeOrder => activeOrder.timeRemaining)
            .FirstOrDefault();

        if (targetOrder == null)
            return;

        heldOrder.DestroySelf();

        Debug.Log($"Completed {targetOrder.order.name} (+{targetOrder.points} pts)");

        ScoreManager.Instance.AddScore(targetOrder.points);

        orderManager.CompleteOrder(targetOrder);
    }
}