using UnityEngine;
using Unity.Netcode;
using System;

public class Furnace : BaseStation, IHasProgress, ITiredness
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    private AudioSource furnaceSoundSource;

    private enum State
    {
        Idle,
        Heating,
        Heated,
        Burnt,
    }

    [SerializeField] private Transform warningSpawnPoint;
    [SerializeField] private UIWarning warningPrefab;
    
    [Header("Recipes")]
    [SerializeField] private FurnaceRecipeSO[] furnaceRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private State state;

    private float heatingTimer;
    private FurnaceRecipeSO furnaceRecipeSO;

    private float burningTimer;
    private BurningRecipeSO burningRecipeSO;
    
    
    private UIWarning activeWarning;

    //TIREDNESS
    [SerializeField] private float energy;

    private bool isProcessing;
    private bool isFurnaceSoundPlaying = false;

    private void Start()
    {
        state = State.Idle;
    }

    private void Update()
    {
        if (!IsServer) return;
        if (!HasIngredient()) return;
        if(!KornnGameManager.Instance.IsGameRunning()) return;

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
        Debug.Log("Start processing");
        if (isProcessing) return;

        isProcessing = true;
        TriggerStartProcessing();

        if (!isFurnaceSoundPlaying)
        {
            furnaceSoundSource = SoundManager.PlayLoopingSound(SoundType.FurnanceBurning);
            isFurnaceSoundPlaying = true;
        }
    }

    private void StopProcessing()
    {
        if (!isProcessing) return;

        isProcessing = false;
        TriggerStopProcessing();

        if (isFurnaceSoundPlaying)
        {
            SoundManager.StopLoopingSound(furnaceSoundSource);
            furnaceSoundSource = null;
            isFurnaceSoundPlaying = false;
        }
    }

    // ---------------- INTERACTION ----------------

    public override bool CanInteract(PlayerStatusController player)
    {
        if (player == null) return false;

        bool furnaceHasIngredient = HasIngredient();

        if (!furnaceHasIngredient && player.HasIngredient())
        {
            Ingredient ingredient = player.GetIngredient();
            if (ingredient == null) return false;

            return GetFurnaceRecipeSOWithInput(ingredient.GetIngredientSO()) != null;
        }

        if (furnaceHasIngredient && !player.IsHoldingSomething())
            return true;

        return false;
    }
    public override void Interact(PlayerStatusController player)
    {
        if (player == null) return;

        if (!IsServer)
        {
            InteractServerRpc(player.NetworkObjectId);
            return;
        }

        InteractServer(player);
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc(ulong playerNetworkObjectId)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out NetworkObject playerNetworkObject))
            return;

        PlayerStatusController player = playerNetworkObject.GetComponent<PlayerStatusController>();
        if (player == null) return;

        InteractServer(player);
    }

    private void InteractServer(PlayerStatusController player)
    {
        if (!CanInteract(player)) return;

        TriggerInteract();

        if (!HasIngredient())
        {
            TryPlaceIngredient(player);
        }
        else
        {
            TryTakeIngredient(player);
        }
    }
    
    [ClientRpc]
    private void ProgressChangedClientRpc(float progressNormalized)
    {
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = progressNormalized
        });
    }

    private void TryPlaceIngredient(PlayerStatusController player)
    {
        Debug.Log("Trying to place ingredient in furnace");
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
        HideBurningWarningClientRpc();
        ProgressChangedClientRpc(0f);
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

        ProgressChangedClientRpc(heatingTimer / furnaceRecipeSO.heatingTimerMax);

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

        ProgressChangedClientRpc(0f);

        Debug.Log("Object heated");

        // No burning recipe = finished processing
        if (burningRecipeSO == null)
        {
            StopProcessing();
        }
        else
        {
            ShowBurningWarningClientRpc();
        }
    }

    private void TickBurning()
    {
        if (burningRecipeSO == null)
            return;

        burningTimer += Time.deltaTime;

        ProgressChangedClientRpc(burningTimer / burningRecipeSO.burningTimerMax);
        
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
        
        HideBurningWarningClientRpc();
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
    
    // WARNING
    private void ShowBurningWarningClient()
    {
        if (activeWarning != null) return;

        activeWarning = Instantiate(
            warningPrefab,
            warningSpawnPoint.position,
            Quaternion.identity
        );
    
        activeWarning.ShowFlicker();
    }

    private void HideBurningWarningClient()
    {
        if (activeWarning == null) return;

        activeWarning.Hide();
        activeWarning = null;
    }

    [ClientRpc]
    private void ShowBurningWarningClientRpc()
    {
        ShowBurningWarningClient();
    }

    [ClientRpc]
    private void HideBurningWarningClientRpc()
    {
        HideBurningWarningClient();
    }
}