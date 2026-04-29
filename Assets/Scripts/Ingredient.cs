using Unity.Netcode;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    [SerializeField] private IngredientSO ingredientSO;

    private IIngredientParent ingredientParent;

    public IngredientSO GetIngredientSO()
    {
        return ingredientSO;
    }

    public void SetIngredientParent(IIngredientParent parent)
    {
        // Clear old parent first
        if (ingredientParent != null)
        {
            ingredientParent.ClearIngredient();
        }

        // Assign new parent
        ingredientParent = parent;

        if (ingredientParent.HasIngredient())
        {
            Debug.LogError("IIngredientParent already has an ingredient");
        }

        // Tell new parent it now owns this ingredient
        parent.SetIngredient(this);

        // Move visually to follow point
        transform.SetParent(parent.GetIngredientFollowTransform());
        transform.localPosition = Vector3.zero;

        Debug.Log($"Parenting ingredient to: {parent.GetIngredientFollowTransform().name}");
    }
    public IIngredientParent GetIngredientParent()
    {
        return ingredientParent;
    }

    public void DestroySelf()
    {
        ingredientParent.ClearIngredient();
        Destroy(gameObject);
    }

    public static Ingredient SpawnIngredient(IngredientSO ingredientSO, IIngredientParent ingredientParent)
    {
        Transform ingredientTransform = Instantiate(ingredientSO.prefab);

        Ingredient ingredient = ingredientTransform.GetComponent<Ingredient>();

        ingredient.SetIngredientParent(ingredientParent);

        return ingredient;
    }
}
