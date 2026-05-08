using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CraftingStationController : NetworkBehaviour, IPlayerInteractable
{
    [Header("Setup")]
    [SerializeField] private int maxIngredients = 3;
    [SerializeField] private CraftingRecipeDatabaseSO recipeDatabase;

    [Header("UI")]
    [SerializeField] private CraftingStationUI craftingUI;

    public List<Ingredient> CurrentIngredients { get; private set; } = new();

    public CraftingRecipeSO CurrentRecipePreview { get; private set; }
    public OrderData CraftedOrder { get; private set; }

    public event Action OnCraftingChanged;

    private void Awake()
    {
        CurrentIngredients.Clear();

        for (int i = 0; i < maxIngredients; i++)
            CurrentIngredients.Add(null);
    }

    // ---------------- INTERACTION ----------------

    public void Interact(PlayerStatusController player)
    {
        if (!IsOwner) return;

        if (craftingUI == null)
        {
            Debug.LogWarning("Crafting UI not assigned!");
            return;
        }

        craftingUI.Show(this, player);
    }

    public bool CanInteract(PlayerStatusController player)
    {
        return true;
    }

    public void Highlight()
    {
        return;
    }

    public void UnHighlight()
    {
        return;
        
    }

    // ---------------- SLOT LOGIC ----------------

    public void ToggleIngredientSlot(int index, PlayerStatusController player)
    {
        if (index < 0 || index >= CurrentIngredients.Count) return;
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
        if (player == null) return;
        if (!player.HasIngredient()) return;

        Ingredient ingredient = player.GetIngredient();
        if (ingredient == null) return;

        CurrentIngredients[index] = ingredient;

        player.ClearIngredient();
        ingredient.gameObject.SetActive(false);
    }

    private void RemoveIngredient(int index, PlayerStatusController player)
    {
        if (player == null) return;
        if (player.HasIngredient()) return;

        Ingredient ingredient = CurrentIngredients[index];
        if (ingredient == null) return;

        CurrentIngredients[index] = null;

        ingredient.gameObject.SetActive(true);
        player.SetIngredient(ingredient);
    }

    // ---------------- MATCHING ----------------

    private void UpdatePreview()
    {
        CurrentRecipePreview = recipeDatabase != null
            ? recipeDatabase.GetMatchingRecipe(CurrentIngredients)
            : null;
    }

    // ---------------- CRAFT ----------------

    public void Craft()
    {
        if (CurrentRecipePreview == null) return;
        if (CraftedOrder != null) return;

        foreach (Ingredient ingredient in CurrentIngredients)
        {
            if (ingredient != null)
                Destroy(ingredient.gameObject);
        }

        for (int i = 0; i < CurrentIngredients.Count; i++)
            CurrentIngredients[i] = null;

        CraftedOrder = CurrentRecipePreview.resultOrder;
        CurrentRecipePreview = null;

        OnCraftingChanged?.Invoke();
    }

    public void TakeCraftedResult(PlayerStatusController player)
    {
        if (player == null) return;
        if (CraftedOrder == null) return;
        if (player.IsHoldingSomething()) return;
        if (CraftedOrder.prefab == null)
        {
            Debug.LogError($"Order prefab missing on {CraftedOrder.orderName}");
            return;
        }

        Order.SpawnOrder(CraftedOrder, player, CraftedOrder.prefab);

        CraftedOrder = null;

        UpdatePreview();
        OnCraftingChanged?.Invoke();
    }

    // ---------------- HELPERS ----------------

    public Ingredient GetIngredient(int index)
    {
        if (index < 0 || index >= CurrentIngredients.Count)
            return null;

        return CurrentIngredients[index];
    }

    public bool HasPreview() => CurrentRecipePreview != null;
    public bool HasCrafted() => CraftedOrder != null;
}