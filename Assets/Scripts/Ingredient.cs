using UnityEngine;

public class Ingredient : MonoBehaviour
{
    [SerializeField] private IngredientSO ingredientSO;

    private IIngredientParent ingredientParent;

    public IngredientSO GetIngredientSO()
    {
        return ingredientSO;
    }

    public void SetIngredientParent(IIngredientParent ingredientParent)
    {
        if (this.ingredientParent != null)
        {
            //this.ingredientParent
        }

        this.ingredientParent = ingredientParent;

        //if (ingredientParent)
        {
            Debug.LogError ("IIngredientParent already has Ingredient");
        }
    }

    public IIngredientParent GetIngredientParent()
    {
        return ingredientParent;
    }
}
