using UnityEngine;

public class DiscardIngredientCommand : ICommand
{
    private Ingredient ingredient;

    public DiscardIngredientCommand(Ingredient ingredient)
    {
        this.ingredient = ingredient;
    }

    public void Execute()
    {
        if(ingredient != null)
        {
            ingredient.DestroySelf();
        }
    }
}
