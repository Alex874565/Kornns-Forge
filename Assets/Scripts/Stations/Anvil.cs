using System;
using Unity.Netcode;
using UnityEngine;

public class Anvil : BaseStation, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    [SerializeField] private AnvilRecipeSO[] anvilRecipeSOArray;

    [Header("Tiredness")]
    [SerializeField] private float energy = 5f;

    private int hammeringProgress;

    public override bool CanInteract(PlayerStatusController player)
    {
        if (player == null) return false;

        bool playerHasIngredient = player.HasIngredient();
        bool playerHoldingSomething = player.IsHoldingSomething();
        bool anvilHasIngredient = HasIngredient();

        if (!anvilHasIngredient && playerHasIngredient)
        {
            Ingredient ingredient = player.GetIngredient();
            if (ingredient == null) return false;

            return HasRecipeWithInput(ingredient.GetIngredientSO());
        }

        if (anvilHasIngredient && !playerHoldingSomething)
            return true;

        return false;
    }

    public override void Interact(PlayerStatusController player)
    {
        if (player == null) return;

        if (!IsServer)
        {
            InteractServerRpc(player.NetworkObjectId);
            return;
        }

        InteractServer(player);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc(ulong playerNetworkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out NetworkObject playerNetworkObject))
            return;

        PlayerStatusController player = playerNetworkObject.GetComponent<PlayerStatusController>();
        if (player == null) return;

        InteractServer(player);
    }

    private void InteractServer(PlayerStatusController player)
    {
        if (!CanInteract(player)) return;

        TriggerInteract();

        if (!HasIngredient())
        {
            Ingredient ingredient = player.GetIngredient();
            if (ingredient == null) return;

            AnvilRecipeSO anvilRecipeSO = GetAnvilRecipeSOWithInput(ingredient.GetIngredientSO());
            if (anvilRecipeSO == null) return;

            player.GetTired(energy);

            ingredient.SetIngredientParent(this);

            hammeringProgress = 0;
            ProgressChangedClientRpc(0f);
        }
        else
        {
            if (player.IsHoldingSomething()) return;

            Ingredient ingredient = GetIngredient();
            if (ingredient == null) return;

            player.GetTired(energy * 0.5f);

            ingredient.SetIngredientParent(player);
        }
    }

    public override void InteractAlternate(PlayerStatusController player)
    {
        if (player == null) return;

        if (!IsServer)
        {
            InteractAlternateServerRpc(player.NetworkObjectId);
            return;
        }

        InteractAlternateServer(player);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractAlternateServerRpc(ulong playerNetworkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out NetworkObject playerNetworkObject))
            return;

        PlayerStatusController player = playerNetworkObject.GetComponent<PlayerStatusController>();
        if (player == null) return;

        InteractAlternateServer(player);
    }

    private void InteractAlternateServer(PlayerStatusController player)
    {
        if (!HasIngredient()) return;

        Ingredient ingredient = GetIngredient();
        if (ingredient == null) return;

        AnvilRecipeSO anvilRecipeSO = GetAnvilRecipeSOWithInput(ingredient.GetIngredientSO());
        if (anvilRecipeSO == null) return;

        TriggerInteractAlternateClientRpc();

        hammeringProgress++;

        ProgressChangedClientRpc((float)hammeringProgress / anvilRecipeSO.hammeringProgressMax);

        player.GetTired(energy * 1.5f);

        if (hammeringProgress >= anvilRecipeSO.hammeringProgressMax)
        {
            IngredientSO output = anvilRecipeSO.output;
            if (output == null) return;

            ingredient.DestroySelf();
            Ingredient.SpawnIngredient(output, this);

            hammeringProgress = 0;
            ProgressChangedClientRpc(0f);
        }
    }
    
    [ClientRpc]
    private void TriggerInteractAlternateClientRpc()
    {
        TriggerInteractAlternate();
    }
    
    [ClientRpc]
    private void ProgressChangedClientRpc(float progressNormalized)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = progressNormalized
        });
    }

    private bool HasRecipeWithInput(IngredientSO input)
    {
        return GetAnvilRecipeSOWithInput(input) != null;
    }

    private AnvilRecipeSO GetAnvilRecipeSOWithInput(IngredientSO input)
    {
        if (input == null) return null;

        foreach (AnvilRecipeSO recipe in anvilRecipeSOArray)
        {
            if (recipe != null && recipe.input == input)
                return recipe;
        }

        return null;
    }
}