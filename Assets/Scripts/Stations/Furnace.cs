using UnityEngine;

public class Furnace : BaseStation, ITiredness
{
    private enum State
    {
        Idle,
        Heating,
        Heated,
        Burnt,
    }

    [Header("Recipes")]
    [SerializeField] private FurnaceRecipeSO[] furnaceRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private State state;

    private float heatingTimer;
    private FurnaceRecipeSO furnaceRecipeSO;

    private float burningTimer;
    private BurningRecipeSO burningRecipeSO;

    //TIREDNESS
    [SerializeField] private float energy;

    private void Start()
    {
        state = State.Idle;
    }

    private void Update()
    {
        if (!IsServer) return;
        if (!HasIngredient()) return;

        switch (state)
        {
            case State.Idle:
                break;

            case State.Heating:
                TickHeating();
                break;

            case State.Heated:
                TickBurning();
                break;

            case State.Burnt:
                break;
        }
    }

    // ---------------- INTERACTION ----------------

    public override bool CanInteract(PlayerStatusController player)
    {
        if (player == null) return false;

        bool furnaceHasIngredient = HasIngredient();
        bool playerHasIngredient = player.HasIngredientNetworked();
        bool playerHoldingSomething = player.IsHoldingSomethingNetworked();

        if (!furnaceHasIngredient && playerHasIngredient)
            return true;

        if (furnaceHasIngredient && !playerHoldingSomething)
            return true;

        return GetFurnaceRecipeSOWithInput(player.GetIngredientNetworked()?.GetIngredientSO()) != null;
    }

    public override void Interact(PlayerStatusController player)
    {
        if (!IsServer) return;
        if (player == null) return;

        if (!HasIngredient())
        {
            TryPlaceIngredient(player);
        }
        else
        {
            TryTakeIngredient(player);
        }
    }

    private void TryPlaceIngredient(PlayerStatusController player)
    {
        if (!player.HasIngredient()) return;

        Ingredient playerIngredient = player.GetIngredient();
        if (playerIngredient == null) return;

        IngredientSO input = playerIngredient.GetIngredientSO();
        FurnaceRecipeSO recipe = GetFurnaceRecipeSOWithInput(input);

        if (recipe == null)
            return;

        // For testing: consuming player's energy when they start using the furnace
        if (player != null)
        {
            player.GetTired(this.energy);
        }

        playerIngredient.SetIngredientParent(this);

        furnaceRecipeSO = recipe;
        heatingTimer = 0f;
        state = State.Heating;
    }

    private void TryTakeIngredient(PlayerStatusController player)
    {
        if (player.IsHoldingSomething()) return;

        Ingredient stationIngredient = GetIngredient();
        if (stationIngredient == null) return;

        stationIngredient.SetIngredientParent(player);

        state = State.Idle;
        heatingTimer = 0f;
        burningTimer = 0f;
        furnaceRecipeSO = null;
        burningRecipeSO = null;
    }

    // ---------------- PROCESSING ----------------

    private void TickHeating()
    {
        if (furnaceRecipeSO == null)
        {
            state = State.Idle;
            return;
        }

        heatingTimer += Time.deltaTime;

        if (heatingTimer < furnaceRecipeSO.heatingTimerMax)
            return;

        Ingredient currentIngredient = GetIngredient();
        if (currentIngredient == null)
        {
            state = State.Idle;
            return;
        }

        IngredientSO heatedOutput = furnaceRecipeSO.output;

        currentIngredient.DestroySelf();
        Ingredient.SpawnIngredient(heatedOutput, this);

        state = State.Heated;
        burningTimer = 0f;
        burningRecipeSO = GetBurningRecipeSOWithInput(heatedOutput);

        Debug.Log("Object heated");
    }

    private void TickBurning()
    {
        if (burningRecipeSO == null)
            return;

        burningTimer += Time.deltaTime;

        if (burningTimer < burningRecipeSO.burningTimerMax)
            return;

        Ingredient currentIngredient = GetIngredient();
        if (currentIngredient == null)
        {
            state = State.Idle;
            return;
        }

        IngredientSO burntOutput = burningRecipeSO.output;

        currentIngredient.DestroySelf();
        Ingredient.SpawnIngredient(burntOutput, this);

        state = State.Burnt;

        Debug.Log("Object burnt");
    }

    // ---------------- RECIPES ----------------

    private FurnaceRecipeSO GetFurnaceRecipeSOWithInput(IngredientSO input)
    {
        if (input == null) return null;

        foreach (FurnaceRecipeSO recipe in furnaceRecipeSOArray)
        {
            if (recipe != null && recipe.input == input)
                return recipe;
        }

        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(IngredientSO input)
    {
        if (input == null) return null;

        foreach (BurningRecipeSO recipe in burningRecipeSOArray)
        {
            if (recipe != null && recipe.input == input)
                return recipe;
        }

        return null;
    }

    public float GetTired(float energy_points)
    {
        return energy;
    }

    public float GetEnegy(float energy_points)
    {
        return 0;
    }
}