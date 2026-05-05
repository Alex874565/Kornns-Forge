using UnityEngine;

[CreateAssetMenu()]
public class FurnaceRecipeSO : ScriptableObject
{
    public IngredientSO input;
    public IngredientSO output;
    public float heatingTimerMax;
}
