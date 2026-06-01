using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class PlayerStatusController : NetworkBehaviour, IIngredientParent
{
    private Ingredient ingredient;
    private Order order;

    [SerializeField] private Transform holdPoint;

    [SerializeField] private Animator animator;
    [SerializeField] private string animatorSleepingBool = "isSleeping";

    private NetworkVariable<ulong> heldIngredientId = new(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<ulong> heldOrderId = new(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<float> energyLevel = new(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<bool> isSleeping = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public event Action OnStartSleeping;
    public event Action OnStopSleeping;
    public event Action<float, float> OnEnergyChanged;
    public event Action<float> OnGetEnergyLevel;

    public override void OnNetworkSpawn()
    {
        HandleEnergyChanged(0f, energyLevel.Value);
        energyLevel.OnValueChanged += HandleEnergyChanged;
    }

    public override void OnNetworkDespawn()
    {
        energyLevel.OnValueChanged -= HandleEnergyChanged;
    }

    private void HandleEnergyChanged(float oldValue, float newValue)
    {
        OnEnergyChanged?.Invoke(oldValue, newValue);
    }

    public Transform GetIngredientFollowTransform()
    {
        return holdPoint;
    }

    // ---------------- INGREDIENT ----------------

    public void SetIngredient(Ingredient newIngredient)
    {
        TrySetIngredient(newIngredient);
    }

    public bool TrySetIngredient(Ingredient newIngredient)
    {
        if (newIngredient == null) return false;
        if (IsHoldingSomething()) return false;

        ingredient = newIngredient;

        if (IsServer)
            heldIngredientId.Value = newIngredient.NetworkObjectId;

        return true;
    }

    public Ingredient GetIngredient()
    {
        if (heldIngredientId.Value == ulong.MaxValue)
            return null;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                heldIngredientId.Value,
                out NetworkObject obj))
            return null;

        return obj.GetComponent<Ingredient>();
    }

    public bool HasIngredient()
    {
        return heldIngredientId.Value != ulong.MaxValue;
    }

    public void ClearIngredient()
    {
        ingredient = null;

        if (IsServer)
            heldIngredientId.Value = ulong.MaxValue;
    }

    // ---------------- ORDER ----------------

    public Order GetOrder()
    {
        if (heldOrderId.Value == ulong.MaxValue)
            return null;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                heldOrderId.Value,
                out NetworkObject obj))
            return null;

        return obj.GetComponent<Order>();
    }

    public bool HasOrder()
    {
        return heldOrderId.Value != ulong.MaxValue;
    }

    public bool TrySetOrder(Order newOrder)
    {
        if (newOrder == null) return false;
        if (IsHoldingSomething()) return false;

        order = newOrder;

        if (IsServer)
            heldOrderId.Value = newOrder.NetworkObjectId;

        return true;
    }
    public void ClearOrder()
    {
        order = null;

        if (IsServer)
            heldOrderId.Value = ulong.MaxValue;
    }

    // ---------------- SHARED ----------------

    public bool IsHoldingSomething()
    {
        return HasIngredient() || HasOrder();
    }

    public void GetTired(float energy_points)
    {
        if (!IsServer) return;

        energyLevel.Value = Mathf.Max(0f, energyLevel.Value - energy_points);
    }

    public void GetEnergy(float energy_points)
    {
        if (!IsServer) return;

        energyLevel.Value = Mathf.Min(100f, energyLevel.Value + energy_points);
    }

    public float GetEnergyLevel()
    {
        OnGetEnergyLevel?.Invoke(energyLevel.Value);
        return energyLevel.Value;
    }

    public void StartSleeping(float duration, Vector3 position)
    {
        OnStartSleeping?.Invoke();

        if (!IsOwner) return;

        PlayerInputController input = GetComponent<PlayerInputController>();
        if (input != null)
            input.SetActive(false);

        PlayerMovementController movement = GetComponent<PlayerMovementController>();
        if (movement != null)
        {
            movement.StopMovement();
            movement.MoveTo(position);
        }
    }

    public void StopSleeping()
    {
        OnStopSleeping?.Invoke();

        if (!IsOwner) return;

        PlayerInputController input = GetComponent<PlayerInputController>();
        if (input != null)
            input.SetActive(true);

        PlayerMovementController movement = GetComponent<PlayerMovementController>();
        if (movement != null)
            movement.StartMovement();
    }

    // --- Stun handling (server -> owner client) ---
    private Coroutine stunCoroutine;

    [ServerRpc(RequireOwnership = false)]
    public void RequestStunServerRpc(float duration)
    {
        // Target the owner client to start the stun locally
        var rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { OwnerClientId } }
        };

        StartStunClientRpc(duration, rpcParams);
    }

    [ClientRpc]
    private void StartStunClientRpc(float duration, ClientRpcParams clientRpcParams = default)
    {
        // Only run on the client that owns this player
        StartSleeping(duration, transform.position);

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(EndStunAfter(duration));
    }

    private IEnumerator EndStunAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        StopSleeping();
        stunCoroutine = null;
    }
}