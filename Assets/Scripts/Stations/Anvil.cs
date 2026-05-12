using UnityEngine;

public class Anvil : BaseStation
{
    [SerializeField] private AnvilRecipeSO[] anvilRecipeSOArray;

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

        if (!HasIngredient())
        {
            if (!player.HasIngredient()) return;

            Ingredient ingredient = player.GetIngredient();
            if (ingredient == null) return;

            ingredient.SetIngredientParent(this);
        }
        else
        {
            if (player.IsHoldingSomething()) return;

            Ingredient ingredient = GetIngredient();
            if (ingredient == null) return;

            ingredient.SetIngredientParent(player);
        }
    }

    public override void InteractAlternate(PlayerStatusController player)
    {
        if (!IsServer) return;
        if (!HasIngredient()) return;

        Ingredient ingredient = GetIngredient();
        if (ingredient == null) return;

        IngredientSO input = ingredient.GetIngredientSO();
        IngredientSO output = GetOutputForInput(input);

        if (output == null)
        {
            Debug.LogError("No matching anvil recipe found!");
            return;
        }

        ingredient.DestroySelf();
        Ingredient.SpawnIngredient(output, this);
    }

    private IngredientSO GetOutputForInput(IngredientSO input)
    {
        if (input == null) return null;

        foreach (AnvilRecipeSO recipe in anvilRecipeSOArray)
        {
            if (recipe == null || recipe.input == null)
                continue;

            if (recipe.input == input)
                return recipe.output;
        }

        return null;
    }
}