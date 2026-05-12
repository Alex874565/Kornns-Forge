using System;
using Unity.Netcode;
using UnityEngine;

public class BaseStation : NetworkBehaviour, IPlayerInteractable, IIngredientParent
{
    [SerializeField] private Transform stationTopPoint;

    protected Ingredient ingredient;

    public Action OnStartProcessing;
    public Action OnStopProcessing;
    public Action OnInteract;

    public event Action OnHighlight, OnUnHighlight;
    [SerializeField] private SelectedCounterVisual selectedVisual;

    public virtual bool CanInteract(PlayerStatusController playerStatusController)
    {
        throw new System.NotImplementedException();
    }

    public virtual void Interact(PlayerStatusController playerStatusController)
    {
        Debug.LogError("BaseStation.Interact();");
    }

    public virtual void InteractAlternate(PlayerStatusController playerStatusController)
    {
        Debug.LogError("BaseStation.InteractAlternate();");
    }

    public void Highlight()
    {
        Debug.Log("Highlighting " + gameObject.name);
        OnHighlight?.Invoke();

        if(selectedVisual != null)
            selectedVisual.Show();
    }

    public void UnHighlight()
    {
        Debug.Log("Unhighlight " + gameObject.name);
        OnUnHighlight?.Invoke();

        if(selectedVisual != null)
            selectedVisual.Hide();
    }

    public Transform GetIngredientFollowTransform()
    {
        return stationTopPoint;
    }

    public void SetIngredient(Ingredient ingredient)
    {
        this.ingredient = ingredient;
    }

    public Ingredient GetIngredient()
    {
        return ingredient;
    }

    public void ClearIngredient()
    {
        ingredient = null;
    }

    public bool HasIngredient()
    {
        return ingredient != null;
    }
}
