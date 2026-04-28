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
        ingredientParent = parent; // ❗ missing line

        transform.SetParent(parent.GetIngredientFollowTransform());
        Debug.Log("Moved ingredient manually to hold point");
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        parent.SetIngredient(this);

        Debug.Log($"Parenting ingredient to: {parent.GetIngredientFollowTransform().name}");
        Debug.Log($"IsOwner: {NetworkManager.Singleton.LocalClientId}");
    }

    public IIngredientParent GetIngredientParent()
    {
        return ingredientParent;
    }
}
