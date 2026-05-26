using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BedStation : BaseStation, ITiredness
{
    [SerializeField] private Sprite sprite;
    [Header("Tiredness")]
    [SerializeField] private float energy = 25f; // total energy to restore over duration
    [SerializeField] private float sleepDuration = 5f; // seconds player stays in bed

    private NetworkVariable<ulong> occupantId = new(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool IsOccupied => occupantId.Value != ulong.MaxValue;
    private Coroutine sleepCoroutine;

    
    public override bool CanInteract(PlayerStatusController player)
    {
        if (player == null) return false;
        if (IsOccupied) return false;
        if (player.IsHoldingSomething()) return false;
        return player.GetEnergyLevel() < 100f;
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
        if (player == null) return;
        if (IsOccupied) return;
        if (!CanInteract(player)) return;

        occupantId.Value = player.NetworkObjectId;

        StartSleepingClientRpc(player.NetworkObjectId);

        if (sleepCoroutine != null)
            StopCoroutine(sleepCoroutine);

        sleepCoroutine = StartCoroutine(HandleSleepRoutine(player, sleepDuration, energy));
    }

    [ClientRpc]
    private void StartSleepingClientRpc(ulong playerNetworkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out NetworkObject playerNetworkObject))
            return;

        PlayerStatusController player = playerNetworkObject.GetComponent<PlayerStatusController>();
        if (player == null) return;
        
        TriggerInteract(); // particles on every client
        
        player.StartSleeping(sleepDuration, transform.position);
    }

    [ClientRpc]
    private void StopSleepingClientRpc(ulong playerNetworkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out NetworkObject playerNetworkObject))
            return;

        PlayerStatusController player = playerNetworkObject.GetComponent<PlayerStatusController>();
        if (player == null) return;

        player.StopSleeping();
    }

    private IEnumerator HandleSleepRoutine(PlayerStatusController player, float duration, float totalEnergy)
    {
        if (player == null)
        {
            occupantId.Value = ulong.MaxValue;
            yield break;
        }

        float elapsed = 0f;
        float interval = 1f; // tick per second
        float perTick = (totalEnergy / Mathf.Max(1f, duration)) * interval;

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(interval);
            if (player != null)
                player.GetEnergy(perTick);
            elapsed += interval;
        }

        // Ensure full amount applied (small remainder)
        float finalEnergy = totalEnergy - (perTick * Mathf.Floor(duration / interval));
        if (finalEnergy > 0f && player != null)
            player.GetEnergy(finalEnergy);

        // Free bed and notify client to stop sleeping animation/input lock
        if (player != null)
            StopSleepingClientRpc(player.NetworkObjectId);

        TriggerStopProcessing();
        occupantId.Value = ulong.MaxValue;
        sleepCoroutine = null;
    }

    // ITiredness compatibility methods - beds primarily recharge
    public float GetTired(float energy_points) => 0f;
    public float GetEnegy(float energy_points) => energy_points;
}
