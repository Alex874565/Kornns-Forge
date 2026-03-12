using Unity.Netcode;
using System;
using UnityEngine;

public class PickupStationController : NetworkBehaviour, IAmPlayerInteractable, IGiveElement
{
    [SerializeField] private PickupStationStats stats;
    
    public event Action OnHighlight, OnUnHighlight, OnInteract;

    public bool InteractOnlyOnce { get; set; } = true;

    public void GiveElement(ElementData element, IReceiveElement receiver)
    {
        receiver.ReceiveElement(element);
    }
    
    #region Interaction
    
    public void Highlight()
    {
        Debug.Log("Highlighting element " + gameObject.name);
        OnHighlight?.Invoke();
    }

    public void UnHighlight()
    {
        Debug.Log("Unhighlighting element " + gameObject.name);
        OnUnHighlight?.Invoke();
    }

    public bool CanInteract(PlayerStatusController playerStatusController)
    {
        return true;
    }

    public void Interact(PlayerStatusController playerStatusController)
    {
        if(!IsOwner) return;
        if(!CanInteract(playerStatusController)) return;

        Debug.Log("Interacting with pickup station " + gameObject.name);
        ElementData elementToGive = stats.HeldElement;
        GiveElement(stats.HeldElement, playerStatusController);
        UnHighlight();
        OnInteract?.Invoke();
    }

    #endregion
}