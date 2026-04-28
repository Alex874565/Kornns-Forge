using UnityEngine;
using System;
using Unity.Netcode;
using System.Collections.Generic;

public class CraftingStationController : NetworkBehaviour, IPlayerInteractable, IHighlightable, IReceiveElement, IGiveElement
{
    [SerializeField] private int maxElements;
    [SerializeField] private List<MaterialData> acceptedElements = new();
    
    private CraftingStationUI craftingStationUI;
    
    private readonly List<MaterialData> _currentElements = new(3);

    public event Action<MaterialData> OnReceiveElement;
    public event Action<MaterialData> OnGiveElement;

    public event Action OnHighlight;
    public event Action OnUnHighlight;
    public event Action<CraftingStationController, PlayerStatusController> OnInteract;

    //public bool InteractOnlyOnce { get; set; } = true;

    private void OnNetworkSpawn()
    {
        CraftingStationUI craftingStationUI = FindFirstObjectByType<CraftingStationUI>();
    }
    
    #region Crafting

    public void GetCraftResult(MaterialData material)
    {
        
    }

    #endregion
    
    #region Give/Receive Element
    
    public void ReceiveElement(MaterialData material)
    {
        if (CanReceiveElement(material))
        {
            _currentElements.Add(material);
            OnReceiveElement?.Invoke(material);
        }
        else
        {
            Debug.Log("Element " + material.Type + " " + material.State + " is not accepted by this crafting station!");
        }
    }

    public bool CanReceiveElement(MaterialData material)
    {
        return IsElementAccepted(material);
    }

    public void GiveElement(MaterialData material, IReceiveElement receiver)
    {
        _currentElements.Remove(material);
        receiver.ReceiveElement(material);
        OnGiveElement?.Invoke(material);
    }
    
    #endregion
    
    #region Interaction
    
    public void Highlight()
    {
        Debug.Log("Crafting station highlighted!");
        OnHighlight.Invoke();
    }

    public void UnHighlight()
    {
        Debug.Log("Crafting station unhighlighted!");
        OnUnHighlight.Invoke();
    }

    public bool CanInteract(PlayerStatusController playerStatusController)
    {
        return CanReceiveElement(playerStatusController.HeldElement.Value);
    }

    public void Interact(PlayerStatusController playerStatusController)
    {
        if (!IsOwner) return;
        if (!CanInteract(playerStatusController)) return;

        OnInteract?.Invoke(this, playerStatusController);
    }
    
    #endregion

    #region Helpers

    private bool IsElementAccepted(MaterialData material)
    {
        return acceptedElements.Exists(e => e.Equals(material)) && _currentElements.Count < 3;
    }

    #endregion

}