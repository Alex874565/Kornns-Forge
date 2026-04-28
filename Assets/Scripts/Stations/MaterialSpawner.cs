using System;
using UnityEngine;

public class MaterialSpawner : BaseStation, IIngredientParent
{
    [SerializeField] private IngredientSO ingredientSO;

    public override bool CanInteract(PlayerStatusController player)
    {
        return !player.HasIngredient();
    }

    // public override void Interact(PlayerStatusController player)
    // {
    //     Debug.Log("interact " + CanInteract(player));
    //     if (!CanInteract(player)) return;

    //     Transform ingredientTransform = Instantiate(ingredientSO.prefab).transform;
    //     Ingredient spawnedIngredient = ingredientTransform.GetComponent<Ingredient>();

    //     spawnedIngredient.SetIngredientParent(player);
    // }

    // public override void Interact(PlayerStatusController player)
    // {
    //     Transform ingredientTransform = Instantiate(ingredientSO.prefab);
    //     ingredientTransform.GetComponent<Ingredient>().SetIngredientParent(player);
    // }

    public override void Interact(PlayerStatusController player)
    {
        Debug.Log("Interacted with material spawner");
        if (!CanInteract(player)) return;

        Transform ingredientTransform = Instantiate(ingredientSO.prefab);

        Debug.Log("Spawned at: " + ingredientTransform.position);

        Ingredient ingredient = ingredientTransform.GetComponent<Ingredient>();

        ingredient.SetIngredientParent(player);
    }
}
