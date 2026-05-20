using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerStatusController : NetworkBehaviour, IIngredientParent
{
    private Ingredient ingredient;
    private Order order;
    [SerializeField] private float energy_level = 100;
    [SerializeField] private Slider energy_bar;

    [SerializeField] private Transform holdPoint;

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
        energy_level -= energy_points;
        if (energy_level < 0)
        {
            energy_level = 0f;
            Debug.Log("Energy depleted. Consider sleeping or restarting.");
        }

        if (updateEnergyCoroutine != null)
        {
            StopCoroutine(updateEnergyCoroutine);
        }
        updateEnergyCoroutine = StartCoroutine(UpdateEnergyBar());
    }

    public void GetEnegy(float energy_points)
    {
        energy_level += energy_points;
        if (energy_level > 100f)
        {
            energy_level = 100f;
            Debug.Log("Maximum Energy reached.");
        }

        if (updateEnergyCoroutine != null)
        {
            StopCoroutine(updateEnergyCoroutine);
        }
        updateEnergyCoroutine = StartCoroutine(UpdateEnergyBar());
    }

    public float GetEnergyLevel()
    {
        return energy_level;
    }

    private IEnumerator UpdateEnergyBar()
    {
        float time = 0;
        float duration = 0.5f;
        float startValue = energy_bar.value;

        while (time < duration)
        {
            energy_bar.value = Mathf.Lerp(startValue, energy_level, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        energy_bar.value = energy_level;
    }

    [SerializeField] private Animator animator;
    [SerializeField] private string animatorSleepingBool = "isSleeping";

    [ClientRpc]
    public void StartSleepingClientRpc(float duration)
    {
        if (!IsOwner) return;

        PlayerInputController input = GetComponent<PlayerInputController>();
        if (input != null)
            input.SetActive(false);

        PlayerMovementController movement = GetComponent<PlayerMovementController>();
        if (movement != null)
            movement.IsInteracting = true;

        PlayerInteractionController interaction = GetComponent<PlayerInteractionController>();
        if (interaction != null)
            interaction.IsInteracting = true;

        Animator anim = animator != null ? animator : GetComponentInChildren<Animator>();
        if (anim != null && !string.IsNullOrEmpty(animatorSleepingBool))
        {
            anim.SetBool(animatorSleepingBool, true);
        }

        StartCoroutine(EndLocalSleepAfter(duration));
    }

    private IEnumerator EndLocalSleepAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        StopSleepingLocal();
    }

    [ClientRpc]
    public void StopSleepingClientRpc()
    {
        if (!IsOwner) return;
        StopSleepingLocal();
    }

    private void StopSleepingLocal()
    {
        PlayerInputController input = GetComponent<PlayerInputController>();
        if (input != null)
            input.SetActive(true);

        PlayerMovementController movement = GetComponent<PlayerMovementController>();
        if (movement != null)
            movement.IsInteracting = false;

        PlayerInteractionController interaction = GetComponent<PlayerInteractionController>();
        if (interaction != null)
            interaction.IsInteracting = false;

        Animator anim = animator != null ? animator : GetComponentInChildren<Animator>();
        if (anim != null && !string.IsNullOrEmpty(animatorSleepingBool))
        {
            anim.SetBool(animatorSleepingBool, false);
        }
    }
}