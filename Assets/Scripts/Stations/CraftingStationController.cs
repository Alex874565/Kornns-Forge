using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CraftingStationController : NetworkBehaviour, IPlayerInteractable
{
    [Header("Setup")]
    [SerializeField] private int maxIngredients = 3;
    [SerializeField] private OrdersDatabase _ordersDatabase;

    [Header("UI")]
    [SerializeField] private CraftingStationUI craftingUI;

    public List<Ingredient> CurrentIngredients { get; private set; } = new();

    public OrderData OrderPreview { get; private set; }
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
        Debug.Log("CRAFTING INTERACT CALLED");

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
    
    public void OpenUIClientOnly(PlayerStatusController player)
    {
        if (craftingUI == null)
        {
            Debug.LogWarning("Crafting UI not assigned!");
            return;
        }

        craftingUI.Show(this, player);
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
    
    public void RequestToggleIngredientSlot(int index, PlayerStatusController player)
    {
        ToggleIngredientSlotServerRpc(index, player.NetworkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleIngredientSlotServerRpc(int index, ulong playerId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(playerId, out NetworkObject playerObj))
            return;

        PlayerStatusController player = playerObj.GetComponent<PlayerStatusController>();
        if (player == null) return;

        ToggleIngredientSlot(index, player);

        RefreshUIClientRpc(player.OwnerClientId);
    }

    // ---------------- MATCHING ----------------

    private void UpdatePreview()
    {
        OrderPreview = _ordersDatabase.GetCraftableOrder(CurrentIngredients);
    }
    
    [ClientRpc]
    private void RefreshUIClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        OnCraftingChanged?.Invoke();
    }

    // ---------------- CRAFT ----------------

    public void Craft()
    {
        if (OrderPreview == null) return;
        if (CraftedOrder != null) return;

        foreach (Ingredient ingredient in CurrentIngredients)
        {
            if (ingredient != null)
                Destroy(ingredient.gameObject);
        }

        for (int i = 0; i < CurrentIngredients.Count; i++)
            CurrentIngredients[i] = null;

        CraftedOrder = OrderPreview;
        OrderPreview = null;

        OnCraftingChanged?.Invoke();
    }
    
    public void RequestCraft(PlayerStatusController player)
    {
        CraftServerRpc(player.NetworkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CraftServerRpc(ulong playerId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(playerId, out NetworkObject playerObj))
            return;

        PlayerStatusController player = playerObj.GetComponent<PlayerStatusController>();
        if (player == null) return;

        Craft();
        TakeCraftedResultServerRpc(playerId);

        RefreshUIClientRpc(player.OwnerClientId);
    }

    public void RequestTakeCraftedResult(PlayerStatusController player)
    {
        TakeCraftedResultServerRpc(player.NetworkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeCraftedResultServerRpc(ulong playerId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(playerId, out NetworkObject playerObj))
            return;

        PlayerStatusController player = playerObj.GetComponent<PlayerStatusController>();
        if (player == null) return;

        TakeCraftedResult(player);

        RefreshUIClientRpc(player.OwnerClientId);
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

    public bool HasPreview() => OrderPreview != null;
    public bool HasCrafted() => CraftedOrder != null;
}