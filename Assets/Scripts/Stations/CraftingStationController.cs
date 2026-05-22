using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CraftingStationController : NetworkBehaviour, IPlayerInteractable
{
    [Header("Setup")]
    [SerializeField] private int maxIngredients = 3;
    [SerializeField] private OrdersDatabase ordersDatabase;
    [SerializeField] private IngredientsDatabase ingredientsDatabase;

    [Header("UI")]
    [SerializeField] private CraftingStationUI craftingUI;

    private NetworkList<int> ingredientIndexes = new NetworkList<int>();
    private NetworkVariable<int> previewOrderIndex = new(-1);
    private NetworkVariable<int> craftedOrderIndex = new(-1);

    public event Action OnCraftingChanged;

    public override void OnNetworkSpawn()
    {
        ingredientIndexes.OnListChanged += OnIngredientListChanged;
        previewOrderIndex.OnValueChanged += OnOrderStateChanged;
        craftedOrderIndex.OnValueChanged += OnOrderStateChanged;

        if (IsServer)
        {
            if (ingredientIndexes.Count == 0)
            {
                for (int i = 0; i < maxIngredients; i++)
                    ingredientIndexes.Add(-1);
            }
        }

        OnCraftingChanged?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        ingredientIndexes.OnListChanged -= OnIngredientListChanged;
        previewOrderIndex.OnValueChanged -= OnOrderStateChanged;
        craftedOrderIndex.OnValueChanged -= OnOrderStateChanged;
    }

    private void OnIngredientListChanged(NetworkListEvent<int> changeEvent)
    {
        StartCoroutine(RefreshNextFrame());
    }

    private IEnumerator RefreshNextFrame()
    {
        yield return null;
        OnCraftingChanged?.Invoke();
    }

    private void OnOrderStateChanged(int previousValue, int newValue)
    {
        OnCraftingChanged?.Invoke();
    }

    // ---------------- INTERACTION ----------------

    public void Interact(PlayerStatusController player)
    {
        Debug.Log($"{player.name} interacted with crafting station {name}");
        OpenUIClientOnly(player);
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

    public bool CanInteract(PlayerStatusController player)
    {
        return true;
    }

    public void Highlight() { }
    public void UnHighlight() { }

    // ---------------- REQUESTS ----------------

    public void RequestToggleIngredientSlot(int index, PlayerStatusController player)
    {
        if (player == null) return;
        ToggleIngredientSlotServerRpc(index, player.NetworkObjectId);
    }

    public void RequestCraft(PlayerStatusController player)
    {
        if (player == null) return;
        CraftServerRpc(player.NetworkObjectId);
    }

    public void RequestTakeCraftedResult(PlayerStatusController player)
    {
        if (player == null) return;
        TakeCraftedResultServerRpc(player.NetworkObjectId);
    }

    // ---------------- SERVER RPCS ----------------

    [ServerRpc(RequireOwnership = false)]
    private void ToggleIngredientSlotServerRpc(int index, ulong playerId)
    {
        if (!TryGetPlayer(playerId, out PlayerStatusController player))
            return;

        ToggleIngredientSlot(index, player);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CraftServerRpc(ulong playerId)
    {
        if (!TryGetPlayer(playerId, out PlayerStatusController player))
            return;

        if (player.IsHoldingSomething())
        {
            Debug.Log($"{player.name} tried to craft but hand is not empty.");
            return;
        }

        Craft();
        TakeCraftedResult(player);
        craftingUI.Hide();
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeCraftedResultServerRpc(ulong playerId)
    {
        if (!TryGetPlayer(playerId, out PlayerStatusController player))
            return;

        TakeCraftedResult(player);
        craftingUI.Hide();
    }

    // ---------------- SLOT LOGIC ----------------

    private void ToggleIngredientSlot(int index, PlayerStatusController player)
    {
        if (!IsServer) return;
        if (index < 0 || index >= ingredientIndexes.Count) return;
        if (HasCrafted()) return;

        if (ingredientIndexes[index] == -1)
            PlaceIngredient(index, player);
        else
            RemoveIngredient(index, player);

        UpdatePreview();
    }

    private void PlaceIngredient(int index, PlayerStatusController player)
    {
        if (player == null) return;
        if (!player.HasIngredient()) return;

        Ingredient ingredient = player.GetIngredient();
        if (ingredient == null) return;

        IngredientSO ingredientSO = ingredient.GetIngredientSO();
        int ingredientIndex = ingredientsDatabase.GetIndex(ingredientSO);

        Debug.Log(
            $"SERVER station={name}, netId={NetworkObjectId}, " +
            $"set slot {index} to {ingredientIndex}"
        );
        
        if (ingredientIndex < 0)
        {
            Debug.LogError($"Ingredient {ingredientSO.name} is missing from OrdersDatabase.");
            return;
        }

        ingredientIndexes[index] = ingredientIndex;

        player.ClearIngredient();
        ingredient.DestroySelf();
    }

    private void RemoveIngredient(int index, PlayerStatusController player)
    {
        if (player == null) return;
        if (player.IsHoldingSomething()) return;

        int ingredientIndex = ingredientIndexes[index];
        if (ingredientIndex < 0) return;

        IngredientSO ingredientSO = ingredientsDatabase.GetIngredient(ingredientIndex);
        if (ingredientSO == null) return;

        ingredientIndexes[index] = -1;

        Ingredient.SpawnIngredient(ingredientSO, player);
    }

    // ---------------- CRAFTING ----------------

    private void UpdatePreview()
    {
        List<IngredientSO> ingredients = new();

        foreach (int index in ingredientIndexes)
        {
            IngredientSO ingredient =
                ingredientsDatabase.GetIngredient(index);

            if (ingredient != null)
                ingredients.Add(ingredient);
        }

        previewOrderIndex.Value =
            ordersDatabase.GetOrderIndex(
                ordersDatabase.GetCraftableOrder(ingredients)
            );
    }

    private void Craft()
    {
        if (!IsServer) return;
        if (!HasPreview()) return;
        if (HasCrafted()) return;
        if (previewOrderIndex.Value == -1) return;
        
        craftedOrderIndex.Value = previewOrderIndex.Value;
        previewOrderIndex.Value = -1;

        for (int i = 0; i < ingredientIndexes.Count; i++)
            ingredientIndexes[i] = -1;
    }

    private void TakeCraftedResult(PlayerStatusController player)
    {
        if (!IsServer) return;
        if (player == null) return;
        if (!HasCrafted()) return;
        if (player.IsHoldingSomething()) return;

        OrderData craftedOrder = GetCraftedOrder();

        if (craftedOrder == null || craftedOrder.prefab == null)
        {
            Debug.LogError("Crafted order prefab missing.");
            return;
        }

        Order.SpawnOrder(craftedOrder, player, craftedOrder.prefab);

        craftedOrderIndex.Value = -1;
        UpdatePreview();
    }

    // ---------------- GETTERS FOR UI ----------------

    public IngredientSO GetIngredient(int index)
    {
        if (ingredientsDatabase == null) return null;
        if (index < 0 || index >= ingredientIndexes.Count) return null;

        return ingredientsDatabase.GetIngredient(ingredientIndexes[index]);
    }
    
    public OrderData GetOrderPreview()
    {
        if (ordersDatabase == null) return null;
        return ordersDatabase.GetOrderByIndex(previewOrderIndex.Value);
    }

    public OrderData GetCraftedOrder()
    {
        if (ordersDatabase == null) return null;
        return ordersDatabase.GetOrderByIndex(craftedOrderIndex.Value);
    }

    public bool HasPreview()
    {
        return previewOrderIndex.Value >= 0;
    }

    public bool HasCrafted()
    {
        return craftedOrderIndex.Value >= 0;
    }

    public int GetIngredientIndexRaw(int index)
    {
        if (ingredientIndexes == null)
        {
            Debug.LogError($"{name}: ingredientIndexes is null");
            return -999;
        }

        if (index < 0 || index >= ingredientIndexes.Count)
        {
            Debug.LogError($"{name}: index {index} invalid, count={ingredientIndexes.Count}");
            return -999;
        }

        return ingredientIndexes[index];
    }
    
    // ---------------- HELPERS ----------------

    private bool TryGetPlayer(ulong playerId, out PlayerStatusController player)
    {
        player = null;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
                .TryGetValue(playerId, out NetworkObject playerObj))
            return false;

        player = playerObj.GetComponent<PlayerStatusController>();

        return player != null;
    }
}