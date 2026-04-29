using Unity.Netcode;
using UnityEngine;

public class PlayerStatusController : NetworkBehaviour, IIngredientParent
{
    private Ingredient ingredient;
    private Order order;

    [SerializeField] private Transform holdPoint;

    // ---------------- FOLLOW POINT ----------------

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
        return true;
    }

    public Ingredient GetIngredient()
    {
        return ingredient;
    }

    public void ClearIngredient()
    {
        ingredient = null;
    }

    public bool HasIngredient()
    {
        return ingredient != null;
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
        return true;
    }

    public Order GetOrder()
    {
        return order;
    }

    public void ClearOrder()
    {
        order = null;
    }

    public bool HasOrder()
    {
        return order != null;
    }

    // ---------------- SHARED ----------------

    public bool IsHoldingSomething()
    {
        return ingredient != null || order != null;
    }

    public void ClearHeldItem()
    {
        ingredient = null;
        order = null;
    }
}