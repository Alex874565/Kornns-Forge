using System;
using Unity.Netcode;
using UnityEngine;

public class BaseStation : NetworkBehaviour, IPlayerInteractable, IIngredientParent
{
    [SerializeField] private Transform stationTopPoint;

    protected Ingredient ingredient;

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

    public void Highlight()
    {
        Debug.Log("Highlighting " + gameObject.name);
        OnHighlight?.Invoke();

        selectedVisual.Show();
    }

    public void UnHighlight()
    {
        Debug.Log("Unhighlight " + gameObject.name);
        OnUnHighlight?.Invoke();

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
