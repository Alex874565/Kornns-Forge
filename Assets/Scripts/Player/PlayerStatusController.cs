using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerStatusController : NetworkBehaviour, IIngredientParent
{
    private Ingredient ingredient;
    private Order order;
    [SerializeField] private Slider energy_bar;

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
    
    public override void OnNetworkSpawn()
    {
        energyLevel.OnValueChanged += HandleEnergyChanged;

        if (energy_bar != null)
            energy_bar.value = energyLevel.Value;
    }

    public override void OnNetworkDespawn()
    {
        energyLevel.OnValueChanged -= HandleEnergyChanged;
    }

    private void HandleEnergyChanged(float oldValue, float newValue)
    {
        if (!IsOwner) return;
        
        if (updateEnergyCoroutine != null)
            StopCoroutine(updateEnergyCoroutine);

        updateEnergyCoroutine = StartCoroutine(UpdateEnergyBar(oldValue, newValue));
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
    private Coroutine updateEnergyCoroutine;

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
        return energyLevel.Value;
    }

    private IEnumerator UpdateEnergyBar(float from, float to)
    {
        if (energy_bar == null)
            yield break;

        float time = 0f;
        float duration = 0.5f;

        while (time < duration)
        {
            energy_bar.value = Mathf.Lerp(from, to, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        energy_bar.value = to;
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
}