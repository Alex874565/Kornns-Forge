using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CraftingStationController : NetworkBehaviour
{
    [SerializeField] private int maxIngredients = 3;
    [SerializeField] private List<OrderData> availableOrders;

    public List<Ingredient> CurrentIngredients { get; private set; } = new();

    public OrderData CurrentOrderPreview { get; private set; }
    public OrderData CraftedOrder { get; private set; }

    public event Action OnCraftingChanged;

    private void Awake()
    {
        for (int i = 0; i < maxIngredients; i++)
            CurrentIngredients.Add(null);
    }

    // ---------------- SLOT LOGIC ----------------

    public void ToggleIngredientSlot(int index, PlayerStatusController player)
    {
        if (CraftedOrder != null) return;

        if (CurrentIngredients[index] == null)
            PlaceIngredient(index, player);
        else
            RemoveIngredient(index, player);

        UpdatePreview();
        OnCraftingChanged?.Invoke();
    }

    private void PlaceIngredient(int index, PlayerStatusController player)
    {
        if (!player.HasIngredient()) return;

        Ingredient ing = player.GetIngredient();
        CurrentIngredients[index] = ing;

        player.ClearIngredient();
        ing.gameObject.SetActive(false);
    }

    private void RemoveIngredient(int index, PlayerStatusController player)
    {
        if (player.HasIngredient()) return;

        Ingredient ing = CurrentIngredients[index];
        if (ing == null) return;

        CurrentIngredients[index] = null;

        ing.gameObject.SetActive(true);
        player.SetIngredient(ing);
    }

    // ---------------- MATCHING ----------------

    private void UpdatePreview()
    {
        CurrentOrderPreview = GetMatchingOrder();
    }

    private OrderData GetMatchingOrder()
    {
        foreach (var order in availableOrders)
        {
            if (Matches(order))
                return order;
        }

        return null;
    }

    private bool Matches(OrderData order)
    {
        List<IngredientSO> input = new();

        foreach (var ing in CurrentIngredients)
        {
            if (ing != null)
                input.Add(ing.GetIngredientSO());
        }

        List<IngredientSO> required = new();

        foreach (var req in order.requirements)
        {
            for (int i = 0; i < req.quantity; i++)
                required.Add(req.ingredient);
        }

        if (input.Count != required.Count)
            return false;

        foreach (var r in required)
        {
            if (input.FindAll(i => i == r).Count !=
                required.FindAll(i => i == r).Count)
                return false;
        }

        return true;
    }

    // ---------------- CRAFT ----------------

    public void Craft()
    {
        if (CurrentOrderPreview == null) return;
        if (CraftedOrder != null) return;

        foreach (var ing in CurrentIngredients)
        {
            if (ing != null)
                Destroy(ing.gameObject);
        }

        for (int i = 0; i < CurrentIngredients.Count; i++)
            CurrentIngredients[i] = null;

        CraftedOrder = CurrentOrderPreview;
        CurrentOrderPreview = null;

        OnCraftingChanged?.Invoke();
    }

    public void TakeCraftedResult(PlayerStatusController player)
    {
        if (CraftedOrder == null) return;
        if (player.HasIngredient()) return;

        Debug.Log("Crafted: " + CraftedOrder.orderName);

        // Optional: reward player
        // player.AddMoney(CraftedOrder.reward);

        CraftedOrder = null;

        OnCraftingChanged?.Invoke();
    }

    // ---------------- HELPERS ----------------

    public Ingredient GetIngredient(int index)
    {
        return CurrentIngredients[index];
    }

    public bool HasPreview() => CurrentOrderPreview != null;
    public bool HasCrafted() => CraftedOrder != null;
}