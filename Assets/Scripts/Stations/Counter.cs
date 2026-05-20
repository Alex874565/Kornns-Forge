using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Counter : BaseStation
{
    [SerializeField] private OrderManager orderManager;
    [Header("Tiredness")]
    [SerializeField] private float energy = 1f;

    public override bool CanInteract(PlayerStatusController player)
    {
        if (player == null) return false;

        // Client + server safe if HasOrder() uses heldOrderId
        if (!player.HasOrder()) return false;

        return true;
    }

    public override void Interact(PlayerStatusController player)
    {
        if (player == null) return;

        if (!IsServer)
        {
            InteractServerRpc(player.NetworkObjectId);
            return;
        }

        InteractServer(player);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc(ulong playerNetworkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out NetworkObject playerNetworkObject))
            return;

        PlayerStatusController player = playerNetworkObject.GetComponent<PlayerStatusController>();
        if (player == null) return;

        InteractServer(player);
    }

    private void InteractServer(PlayerStatusController player)
    {
        if (!CanInteract(player)) return;

        TriggerInteract();
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

        // small energy cost for fulfilling an order
        player.GetTired(this.energy);

        Debug.Log($"Completed {targetOrder.order.name} (+{targetOrder.points} pts)");

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(targetOrder.points);

        orderManager.CompleteOrder(targetOrder);
    }
}