using System;
using UnityEngine;

public class MaterialSpawner : BaseStation, IIngredientParent
{
    public event EventHandler OnPlayerGrabbedMaterial;

    [SerializeField] private IngredientSO ingredientSO;

    public override bool CanInteract(PlayerStatusController player)
    {
        return !player.HasIngredient();
    }

    public override void Interact(PlayerStatusController player)
    {
        Debug.Log("Interacted with material spawner");
        if (!CanInteract(player)) return;

        if (!player.HasIngredient())
        {
            Ingredient.SpawnIngredient(ingredientSO, player);

            OnPlayerGrabbedMaterial?.Invoke(this, EventArgs.Empty);
        }  
    }
}
