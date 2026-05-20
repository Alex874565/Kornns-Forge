using System;
using Unity.Netcode;
using UnityEngine;

public class BaseStation : NetworkBehaviour, IPlayerInteractable, IIngredientParent
{
    [Header("Ingredient")]
    [SerializeField] private Transform stationTopPoint;

    [Header("Visual")]

    [SerializeField] private SelectedCounterVisual selectedVisual;

    protected Ingredient ingredient;

    private readonly NetworkVariable<ulong> ingredientId = new(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public event Action OnHighlight;
    public event Action OnUnHighlight;
    
    public Action OnStartProcessing;
    public Action OnStopProcessing;
    public Action OnInteract;
    public Action OnInteractAlternate;
    
    // ---------------- NETWORK TRIGGERS ----------------

    public void TriggerInteract()
    {
        if (!IsServer) return;
        OnInteract?.Invoke();
        TriggerInteractClientRpc();
    }

    public void TriggerInteractAlternate()
    {
        if (!IsServer) return;
        OnInteractAlternate?.Invoke();
        TriggerInteractAlternateClientRpc();
    }

    [ClientRpc]
    private void TriggerInteractClientRpc()
    {
        if (IsServer) return;
        OnInteract?.Invoke();
    }

    [ClientRpc]
    private void TriggerInteractAlternateClientRpc()
    {
        if (IsServer) return;
        OnInteractAlternate?.Invoke();
    }

    public void TriggerStartProcessing()
    {
        if (!IsServer) return;
        OnStartProcessing?.Invoke();
        TriggerStartProcessingClientRpc();
    }

    [ClientRpc]
    private void TriggerStartProcessingClientRpc()
    {
        if (IsServer) return;
        OnStartProcessing?.Invoke();
    }

    public void TriggerStopProcessing()
    {
        if (!IsServer) return;
        OnStopProcessing?.Invoke();
        TriggerStopProcessingClientRpc();
    }

    [ClientRpc]
    private void TriggerStopProcessingClientRpc()
    {
        if (IsServer) return;
        OnStopProcessing?.Invoke();
    }

    // ---------------- INTERACTION ----------------

    public virtual bool CanInteract(PlayerStatusController player)
    {
        return false;
    }

    public virtual void Interact(PlayerStatusController player)
    {
        Debug.LogError($"{name}: Interact() not implemented.");
    }

    public virtual void InteractAlternate(PlayerStatusController player)
    {
        Debug.LogError($"{name}: InteractAlternate() not implemented.");
    }

    // ---------------- HIGHLIGHT ----------------

    public void Highlight()
    {
        OnHighlight?.Invoke();

        if (selectedVisual != null)
            selectedVisual.Show();
    }

    public void UnHighlight()
    {
        OnUnHighlight?.Invoke();

        if (selectedVisual != null)
            selectedVisual.Hide();
    }

    // ---------------- INGREDIENT PARENT ----------------

    public Transform GetIngredientFollowTransform()
    {
        return stationTopPoint;
    }

    public void SetIngredient(Ingredient newIngredient)
    {
        ingredient = newIngredient;

        if (!IsServer) return;

        ingredientId.Value = newIngredient != null
            ? newIngredient.NetworkObjectId
            : ulong.MaxValue;
    }

    public void ClearIngredient()
    {
        ingredient = null;

        if (!IsServer) return;

        ingredientId.Value = ulong.MaxValue;
    }

    public bool HasIngredient()
    {
        return ingredientId.Value != ulong.MaxValue;
    }

    public Ingredient GetIngredient()
    {
        if (IsServer)
            return ingredient;

        return GetIngredientFromNetworkId();
    }

    private Ingredient GetIngredientFromNetworkId()
    {
        if (ingredientId.Value == ulong.MaxValue)
            return null;

        if (NetworkManager.Singleton == null)
            return null;

        bool found = NetworkManager.Singleton.SpawnManager.SpawnedObjects
            .TryGetValue(ingredientId.Value, out NetworkObject obj);

        if (!found || obj == null)
            return null;

        return obj.GetComponent<Ingredient>();
    }
}