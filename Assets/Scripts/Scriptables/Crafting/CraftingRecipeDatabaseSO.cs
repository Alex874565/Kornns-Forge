using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe Database")]
public class CraftingRecipeDatabaseSO : ScriptableObject
{
    [SerializeField] private List<CraftingRecipeSO> recipes;

    public CraftingRecipeSO GetMatchingRecipe(List<Ingredient> currentIngredients)
    {
        List<IngredientSO> input = currentIngredients
            .Where(i => i != null)
            .Select(i => i.GetIngredientSO())
            .ToList();

        foreach (CraftingRecipeSO recipe in recipes)
        {
            if (recipe.ingredients.Count != input.Count)
                continue;

            bool matches = recipe.ingredients.All(required =>
                input.Count(i => i == required) ==
                recipe.ingredients.Count(i => i == required)
            );

            if (matches)
                return recipe;
        }

        return null;
    }
}