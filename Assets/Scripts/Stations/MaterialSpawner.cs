using System;
using UnityEngine;

public class MaterialSpawner : MonoBehaviour, IIngredientParent, IPlayerInteractable
{
    [SerializeField] private IngredientSO ingredientSO;
    [SerializeField] private Transform stationTopPoint;

    private Ingredient ingredient;

    public event Action OnHighlight, OnUnHighlight;

    [SerializeField] private SelectedCounterVisual selectedVisual;

    // public void Interact(PlayerInteractionController playerInteractionController)
    // {
    //     Transform ingredientTransform = Instantiate(ingredientSO.prefab, stationTopPoint);
    //     ingredientTransform.GetComponent<Ingredient>().SetIngredientParent(playerInteractionController);

    // }

    public bool CanInteract(PlayerStatusController player)
    {
        return player.HeldElement.Value.Type == MaterialType.None;
    }

    public void Interact(PlayerStatusController player)
    {
        if (!CanInteract(player)) return;

        Transform ingredientTransform = Instantiate(ingredientSO.prefab);
        Ingredient spawnedIngredient = ingredientTransform.GetComponent<Ingredient>();

        spawnedIngredient.SetIngredientParent(player);
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
}
