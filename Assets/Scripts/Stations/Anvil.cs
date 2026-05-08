using UnityEngine;

public class Anvil : BaseStation, IIngredientParent
{
    [SerializeField] private AnvilRecipeSO[] anvilRecipeSOArray;

    public override bool CanInteract(PlayerStatusController player)
    {
        bool playerHasIngredient = player.HasIngredient();
        bool anvilHasIngredient = HasIngredient();

        return playerHasIngredient != anvilHasIngredient;
    }

    public override void Interact(PlayerStatusController playerStatusController)
    {
        if (!HasIngredient())
        {
            //there s no ingredient here 
            if (playerStatusController.HasIngredient())
            {
                //player is carrying something
                playerStatusController.GetIngredient().SetIngredientParent(this);
            } else
            {
                //player isn t carrying anything
            }
        } else
        {
            //there is an ingredient here
            if (playerStatusController.HasIngredient())
            {
                //player is carrying something
            } else
            {
                //player isn t carrying anything
                GetIngredient().SetIngredientParent(playerStatusController);
            }
        }
    }

    public override void InteractAlternate(PlayerStatusController playerStatusController)
    {
        if (!HasIngredient()) return;

        IngredientSO outputIngredientSO = GetOutputForInput(GetIngredient().GetIngredientSO());

        if (outputIngredientSO == null)
        {
            Debug.LogError("No matching anvil recipe found!");
            return;
        }

        GetIngredient().DestroySelf();

        Ingredient.SpawnIngredient(outputIngredientSO, this);
    }

    private IngredientSO GetOutputForInput(IngredientSO inputIngredientSO)
    {
        Debug.Log("Input ingredient: " + inputIngredientSO.name);

        foreach (AnvilRecipeSO anvilRecipeSO in anvilRecipeSOArray)
        {
            Debug.Log("Checking recipe: " + anvilRecipeSO.input.name);

            if (anvilRecipeSO.input == inputIngredientSO)
            {
                Debug.Log("MATCH FOUND");
                return anvilRecipeSO.output;
            }
        }

        Debug.LogError("NO MATCH FOUND");
        return null;
    }
}
