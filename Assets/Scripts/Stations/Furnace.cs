using UnityEngine;
using Unity.Netcode;
using System;

public class Furnace : BaseStation, IHasProgress, ITiredness
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

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

    private bool isProcessing;

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

    // ---------------- PROCESS EVENTS ----------------

    private void StartProcessing()
    {
        if (isProcessing) return;

        isProcessing = true;
        OnStartProcessing?.Invoke();
    }

    private void StopProcessing()
    {
        if (!isProcessing) return;

        isProcessing = false;
        OnStopProcessing?.Invoke();
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

        StartProcessing();
    }

    private void TryTakeIngredient(PlayerStatusController player)
    {
        if (player.IsHoldingSomething()) return;

        Ingredient stationIngredient = GetIngredient();
        if (stationIngredient == null) return;

        stationIngredient.SetIngredientParent(player);

        ResetProcessing();
    }

    private void ResetProcessing()
    {
        state = State.Idle;

        heatingTimer = 0f;
        burningTimer = 0f;

        furnaceRecipeSO = null;
        burningRecipeSO = null;

        StopProcessing();

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
            progressNormalized = 0f
        });
    }

    // ---------------- PROCESSING ----------------

    private void TickHeating()
    {
        if (furnaceRecipeSO == null)
        {
            ResetProcessing();
            return;
        }

        heatingTimer += Time.deltaTime;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                progressNormalized = heatingTimer / furnaceRecipeSO.heatingTimerMax
            });

        if (heatingTimer < furnaceRecipeSO.heatingTimerMax)
            return;

        Ingredient currentIngredient = GetIngredient();
        if (currentIngredient == null)
        {
            ResetProcessing();
            return;
        }

        IngredientSO heatedOutput = furnaceRecipeSO.output;

        currentIngredient.DestroySelf();
        Ingredient.SpawnIngredient(heatedOutput, this);

        state = State.Heated;
        burningTimer = 0f;
        burningRecipeSO = GetBurningRecipeSOWithInput(heatedOutput);

        Debug.Log("Object heated");

        // No burning recipe = finished processing
        if (burningRecipeSO == null)
        {
            StopProcessing();
        }
    }

    private void TickBurning()
    {
        if (burningRecipeSO == null)
            return;

        burningTimer += Time.deltaTime;

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
                progressNormalized = burningTimer / burningRecipeSO.burningTimerMax
            });

        if (burningTimer < burningRecipeSO.burningTimerMax)
            return;

        Ingredient currentIngredient = GetIngredient();
        if (currentIngredient == null)
        {
            ResetProcessing();
            return;
        }

        IngredientSO burntOutput = burningRecipeSO.output;

        currentIngredient.DestroySelf();
        Ingredient.SpawnIngredient(burntOutput, this);

        state = State.Burnt;

        StopProcessing();

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