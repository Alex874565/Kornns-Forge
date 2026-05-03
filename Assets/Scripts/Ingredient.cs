using System;
using Unity.Netcode;
using UnityEngine;

public class Ingredient : MonoBehaviour, IThrowable
{
    [SerializeField] private IngredientSO ingredientSO;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D coll;

    private IIngredientParent ingredientParent;
    private float _throwAngle;

    private void Awake()
    {
        if(rb == null) rb = GetComponent<Rigidbody2D>();
        if (coll == null) coll = GetComponent<Collider2D>();
    }

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
        
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

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

    public void ThrowSelf(Vector2 direction, float force, float angle)
    {
        if (direction == Vector2.zero) return;

        if (ingredientParent != null)
        {
            ingredientParent.ClearIngredient();
            ingredientParent = null;
        }
        
        transform.SetParent(null);
        
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        Vector2 angledDirection = Quaternion.Euler(0, 0, angle) * direction.normalized;
        rb.AddForce(angledDirection * force, ForceMode2D.Impulse);
    }
}
