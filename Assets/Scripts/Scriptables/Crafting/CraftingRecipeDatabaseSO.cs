using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe Database")]
public class CraftingRecipeDatabaseSO : ScriptableObject
{
    [SerializeField] private List<CraftingRecipeSO> recipes = new();

    public CraftingRecipeSO GetMatchingRecipe(List<Ingredient> currentIngredients)
    {
        List<IngredientSO> input = currentIngredients
            .Where(i => i != null)
            .Select(i => i.GetIngredientSO())
            .ToList();

        foreach (CraftingRecipeSO recipe in recipes)
        {
            if (RecipeMatches(recipe, input))
                return recipe;
        }

        return null;
    }

    private bool RecipeMatches(CraftingRecipeSO recipe, List<IngredientSO> input)
    {
        if (recipe == null || recipe.requirements == null)
            return false;

        List<IngredientSO> required = new();

        foreach (OrderRequirement req in recipe.requirements)
        {
            if (req == null || req.ingredient == null)
                continue;

            for (int i = 0; i < req.quantity; i++)
                required.Add(req.ingredient);
        }

        if (required.Count != input.Count)
            return false;

        foreach (IngredientSO ingredient in required)
        {
            int requiredCount = required.Count(i => i == ingredient);
            int inputCount = input.Count(i => i == ingredient);

            if (requiredCount != inputCount)
                return false;
        }

        return true;
    }
}