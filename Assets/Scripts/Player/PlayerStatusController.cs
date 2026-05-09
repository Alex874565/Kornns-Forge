using Unity.Netcode;
using UnityEngine;

public class PlayerStatusController : NetworkBehaviour, IIngredientParent
{
    private Ingredient ingredient;
    private Order order;

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
        return ingredient;
    }

    public Ingredient GetIngredientNetworked()
    {
        if (heldIngredientId.Value == ulong.MaxValue)
            return null;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                heldIngredientId.Value,
                out NetworkObject obj))
            return null;

        return obj.GetComponent<Ingredient>();
    }

    public void ClearIngredient()
    {
        ingredient = null;

        if (IsServer)
            heldIngredientId.Value = ulong.MaxValue;
    }

    public bool HasIngredient()
    {
        return ingredient != null;
    }

    public bool HasIngredientNetworked()
    {
        return heldIngredientId.Value != ulong.MaxValue;
    }

    // ---------------- ORDER ----------------

    public void SetOrder(Order newOrder)
    {
        TrySetOrder(newOrder);
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

    public Order GetOrder()
    {
        return order;
    }

    public void ClearOrder()
    {
        order = null;

        if (IsServer)
            heldOrderId.Value = ulong.MaxValue;
    }

    public bool HasOrder()
    {
        return order != null;
    }

    public bool HasOrderNetworked()
    {
        return heldOrderId.Value != ulong.MaxValue;
    }

    // ---------------- SHARED ----------------

    public bool IsHoldingSomething()
    {
        return ingredient != null || order != null;
    }

    public bool IsHoldingSomethingNetworked()
    {
        return HasIngredientNetworked() || HasOrderNetworked();
    }

    public void ClearHeldItem()
    {
        ClearIngredient();
        ClearOrder();
    }
}