using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerStatusController : NetworkBehaviour, IIngredientParent
{
    //public event Action<MaterialData> OnChangeHeldElement;
    
    private int Tiredness { get; set; }
    
    //public NetworkVariable<MaterialData> HeldElement { get; private set; } = new (writePerm: NetworkVariableWritePermission.Owner);
    //private NetworkVariable<MaterialData>.OnValueChangedDelegate OnChangeHeldElementDelegate { get; set; }

    private Ingredient ingredient;

    [SerializeField] private Transform ingredientHoldPoint;

    public override void OnNetworkSpawn()
    {        
        if (!IsOwner) return;
        //OnChangeHeldElementDelegate= (oldElement, newElement) =>
        //{
        //    Debug.Log("Held element changed to " + newElement);
        //    OnChangeHeldElement?.Invoke(newElement);
        //};
        //HeldElement.OnValueChanged += OnChangeHeldElementDelegate;
        //HeldElement.Value = new MaterialData();
    }

    public Transform GetIngredientFollowTransform()
    {
        Debug.Log($"HoldPoint: {ingredientHoldPoint}");
        return ingredientHoldPoint;
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