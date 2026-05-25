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
        if (player == null || IsOccupied) return;

        // Tell the owner client to enter sleeping state (locks input/animation)
        if (player.IsOwner)
        {
            player.StartSleepingClientRpc(sleepDuration, gameObject.transform.position);
        }

        if (!IsServer) return;

        TriggerInteract();
        TriggerStartProcessing();

        // Occupy the bed with this player
        occupantId.Value = player.NetworkObjectId;

        // Start server-side coroutine to recharge player over time and free bed
        sleepCoroutine = StartCoroutine(HandleSleepRoutine(player, sleepDuration, energy));
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
                player.GetEnegy(perTick);
            elapsed += interval;
        }

        // Ensure full amount applied (small remainder)
        float finalEnergy = totalEnergy - (perTick * Mathf.Floor(duration / interval));
        if (finalEnergy > 0f && player != null)
            player.GetEnegy(finalEnergy);

        // Free bed and notify client to stop sleeping animation/input lock
        if (player != null)
            player.StopSleepingClientRpc();

        TriggerStopProcessing();
        occupantId.Value = ulong.MaxValue;
        sleepCoroutine = null;
    }

    public override void InteractAlternate(PlayerStatusController player)
    {
        if (!IsServer) return;
        if (player == null) return;

        // Allow the occupying player to wake early
        if (!IsOccupied) return;
        if (occupantId.Value != player.NetworkObjectId) return;

        if (sleepCoroutine != null)
        {
            StopCoroutine(sleepCoroutine);
            sleepCoroutine = null;
        }

        // Ensure client restores controls/animation
        player.StopSleepingClientRpc();

        TriggerStopProcessing();
        occupantId.Value = ulong.MaxValue;
        }

    // ITiredness compatibility methods - beds primarily recharge
    public float GetTired(float energy_points) => 0f;
    public float GetEnegy(float energy_points) => energy_points;
}
