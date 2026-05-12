using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Counter : BaseStation
{
    [SerializeField] private OrderManager orderManager;

    public override bool CanInteract(PlayerStatusController player)
    {
        if (player == null) return false;

        // Client + server safe if HasOrder() uses heldOrderId
        if (!player.HasOrderNetworked()) return false;

        return true;
    }

    public override void Interact(PlayerStatusController player)
    {
        if (!IsServer) return;

        SubmitOrder(player);
    }

    private void SubmitOrder(PlayerStatusController player)
    {
        if (player == null) return;
        if (orderManager == null) return;
        if (!player.HasOrder()) return;

        Order heldOrder = player.GetOrder();
        if (heldOrder == null) return;

        OrderData heldOrderData = heldOrder.GetOrderData();
        if (heldOrderData == null) return;

        OrderProgress targetOrder = orderManager.GetActiveOrders()
            .Where(activeOrder =>
                activeOrder != null &&
                !activeOrder.crafted &&
                !activeOrder.IsExpired() &&
                activeOrder.order == heldOrderData
            )
            .OrderBy(activeOrder => activeOrder.timeRemaining)
            .FirstOrDefault();

        if (targetOrder == null)
        {
            Debug.Log("No matching active order found.");
            return;
        }

        heldOrder.DestroySelf();
        player.ClearOrder();

        Debug.Log($"Completed {targetOrder.order.name} (+{targetOrder.points} pts)");

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(targetOrder.points);

        orderManager.CompleteOrder(targetOrder);
    }
}   