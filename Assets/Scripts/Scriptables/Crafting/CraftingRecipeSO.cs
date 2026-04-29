using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class CraftingRecipeSO : ScriptableObject
{
    public List<IngredientSO> ingredients = new();
    public IngredientSO result;
}