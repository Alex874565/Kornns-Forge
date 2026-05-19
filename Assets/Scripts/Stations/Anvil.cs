using System;
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

        bool playerHasIngredient = player.HasIngredientNetworked();
        bool playerHoldingSomething = player.IsHoldingSomethingNetworked();
        bool anvilHasIngredient = HasIngredient();

        if (!anvilHasIngredient && playerHasIngredient)
            return true;

        if (anvilHasIngredient && !playerHoldingSomething)
            return true;

        return false;
    }

    public override void Interact(PlayerStatusController player)
    {
        if (!IsServer) return;
        if (player == null) return;

        TriggerInteract();

        if (!HasIngredient())
        {
            if (!player.HasIngredient()) return;

            Ingredient ingredient = player.GetIngredient();
            if (ingredient == null) return;

            // consume player energy for placing ingredient on anvil
            player.GetTired(this.energy);

            ingredient.SetIngredientParent(this);

            hammeringProgress = 0;

            IngredientSO input = ingredient.GetIngredientSO();
            AnvilRecipeSO anvilRecipeSO = GetAnvilRecipeSOWithInput(input);

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                progressNormalized = (float)hammeringProgress / anvilRecipeSO.hammeringProgressMax
            });
        }
        else
        {
            if (player.IsHoldingSomething()) return;

            Ingredient ingredient = GetIngredient();
            if (ingredient == null) return;

            // taking ingredient from anvil also costs small effort
            player.GetTired(this.energy * 0.5f);

            ingredient.SetIngredientParent(player);
        }
    }

    public override void InteractAlternate(PlayerStatusController player)
    {
        if (!IsServer) return;
        if (!HasIngredient()) return;

        TriggerInteract();

        Ingredient ingredient = GetIngredient();
        if (ingredient == null) return;

        IngredientSO input = ingredient.GetIngredientSO();

        AnvilRecipeSO anvilRecipeSO = GetAnvilRecipeSOWithInput(input);

        hammeringProgress++;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                progressNormalized = (float)hammeringProgress / anvilRecipeSO.hammeringProgressMax
            });

        // crafting on the anvil consumes more energy
        player.GetTired(this.energy * 1.5f);

        /*Check if the hammering finished*/
        if (hammeringProgress >= anvilRecipeSO.hammeringProgressMax)
        {
            IngredientSO output = GetOutputForInput(input);

            if (output == null)
            {
                Debug.LogError("No matching anvil recipe found!");
                return;
            }

            ingredient.DestroySelf();
            Ingredient.SpawnIngredient(output, this);
        }
    }

    private bool HasRecipeWithInput(IngredientSO input)
    {
        AnvilRecipeSO anvilRecipeSO = GetAnvilRecipeSOWithInput(input);
        return anvilRecipeSO != null;
    }

    private IngredientSO GetOutputForInput(IngredientSO input)
    {
        if (input == null) return null;

        AnvilRecipeSO anvilRecipeSO = GetAnvilRecipeSOWithInput(input);

        if (anvilRecipeSO != null)
        {
            return anvilRecipeSO.output;
        } else
        {
            return null;
        }
    }

    private AnvilRecipeSO GetAnvilRecipeSOWithInput(IngredientSO input)
    {
        if (input == null) return null;

        foreach (AnvilRecipeSO recipe in anvilRecipeSOArray)
        {
            if (recipe == null || recipe.input == null)
                continue;

            if (recipe.input == input)
                return recipe;
        }

        return null;
    }
}