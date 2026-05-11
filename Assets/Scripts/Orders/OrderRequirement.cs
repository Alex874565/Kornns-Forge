using UnityEngine;

[System.Serializable]
public class OrderRequirement
{
    public IngredientSO ingredient;
    public int quantity = 1;
    public Sprite processIcon;
}

[System.Serializable]
public class CollectedItem
{
    public IngredientSO ingredient;

    public bool Matches(IngredientSO other)
    {
        return ingredient == other;
    }
}