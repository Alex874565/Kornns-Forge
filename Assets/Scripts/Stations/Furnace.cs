using UnityEngine;
using UnityEngine.AI;

public class Furnace : BaseStation
{
    private enum State
    {
        Idle, 
        Heating, 
        Heated, 
        Burnt,
    }
    [SerializeField] private FurnaceRecipeSO[] furnaceRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private State state;
    private float heatingTimer;
    private FurnaceRecipeSO furnaceRecipeSO;
    private float burningTimer;
    private BurningRecipeSO burningRecipeSO;

    private void Start()
    {
        state = State.Idle;
    }

    private void Update()
    {
        if (HasIngredient())
        {
            switch (state)
            {
                case State.Idle:
                    break;
                case State.Heating:
                    heatingTimer += Time.deltaTime;
                    if (heatingTimer > furnaceRecipeSO.heatingTimerMax)
                    {
                        // heated
                        GetIngredient().DestroySelf();

                        Ingredient.SpawnIngredient(furnaceRecipeSO.output, this);

                        Debug.Log("object heated");

                        state = State.Heated;
                        burningTimer = 0f;
                        burningRecipeSO = GetBurningRecipeSOWithInput(GetIngredient().GetIngredientSO());
                    }
                    break;
                case State.Heated:
                    burningTimer += Time.deltaTime;
                    if (burningTimer > burningRecipeSO.burningTimerMax)
                    {
                        // heated
                        GetIngredient().DestroySelf();

                        Ingredient.SpawnIngredient(burningRecipeSO.output, this);

                        Debug.Log("object burnt");
                        state = State.Burnt;
                    }
                    break;
                case State.Burnt:
                    break;
            }
            Debug.Log(state);
        }
    }

    public override bool CanInteract(PlayerStatusController player)
    {
        bool playerHasIngredient = player.HasIngredient();
        bool furnaceHasIngredient = HasIngredient();

        return playerHasIngredient != furnaceHasIngredient;
    }

    public override void Interact(PlayerStatusController playerStatusController)
    {
        if (!HasIngredient())
        {
            //there s no ingredient here 
            if (playerStatusController.HasIngredient())
            {
                //player is carrying something
                if (HasRecipeWithInput(playerStatusController.GetIngredient().GetIngredientSO()))
                {
                    //player is carrying something that can be heated
                    playerStatusController.GetIngredient().SetIngredientParent(this);

                    furnaceRecipeSO = GetFurnaceRecipeSOWithInput(GetIngredient().GetIngredientSO());

                    state = State.Heating;
                    heatingTimer = 0f;
                }
                
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

    private bool HasRecipeWithInput(IngredientSO inputIngredientSO)
    {
        FurnaceRecipeSO furnaceRecipeSO = GetFurnaceRecipeSOWithInput(inputIngredientSO);
        return furnaceRecipeSO != null;
    }

    private IngredientSO GetOutputForInput(IngredientSO inputIngredientSO)
    {
        FurnaceRecipeSO furnaceRecipeSO = GetFurnaceRecipeSOWithInput(inputIngredientSO);
        if (furnaceRecipeSO != null)    
        {
            return furnaceRecipeSO.output;
        } else
        {
            return null;
        }
    }

    private FurnaceRecipeSO GetFurnaceRecipeSOWithInput(IngredientSO inputIngredientSO)
    {
        foreach (FurnaceRecipeSO furnaceRecipeSO in furnaceRecipeSOArray)
        {
            if (furnaceRecipeSO.input == inputIngredientSO)
            {
                return furnaceRecipeSO;
            }
        }
        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(IngredientSO inputIngredientSO)
    {
        foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray)
        {
            if (burningRecipeSO.input == inputIngredientSO)
            {
                return burningRecipeSO;
            }
        }
        return null;
    }
}
