using UnityEngine;

[CreateAssetMenu(menuName = "Ingredients/Ingredients Database")]
public class IngredientsDatabase : ScriptableObject
{
    public IngredientSO[] ingredients;

    public int GetIndex(IngredientSO ingredient)
    {
        if (ingredient == null)
            return -1;

        return System.Array.IndexOf(ingredients, ingredient);
    }

    public IngredientSO GetIngredient(int index)
    {
        if (index < 0 || index >= ingredients.Length)
            return null;

        return ingredients[index];
    }
}