using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class CraftingRecipeSO : ScriptableObject
{
    public string recipeName;

    public OrderData resultOrder;

    public List<OrderRequirement> requirements = new();
}