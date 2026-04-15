using Unity.Netcode;
using UnityEngine;
using System;

[RequireComponent(typeof(PlayerInputController), typeof(PlayerStatusController))]
public class PlayerInteractionController : NetworkBehaviour
{
    [SerializeField] private LayerMask interactLayerMask;
    [SerializeField] private new Collider2D collider;

    public Action OnBeginInteraction, OnEndInteraction, OnInteract, OnInteractFailed;
    
    private IAmPlayerInteractable _hoveredInteractable;
    
    private PlayerInputController _playerInputController;
    private PlayerStatusController _playerStatusController;
    
    private ContactFilter2D _contactFilter;
    
    public bool IsInteracting { get; set; }
    public bool InteractOnlyOnce { get; set; } = true;
    
    public override void OnNetworkSpawn()
    {
        if(!IsOwner) return;
        _playerInputController = GetComponent<PlayerInputController>();
        _playerStatusController = GetComponent<PlayerStatusController>();
        
        _playerInputController.OnInteract += InteractClick;
        
        _contactFilter = new ContactFilter2D();
        _contactFilter.SetLayerMask(interactLayerMask);
        _contactFilter.useLayerMask = true;
        _contactFilter.useTriggers = true;
        
        OnInteract += Interact;
    }

    private void FixedUpdate()
    {
        if(!IsOwner) return;
        
        IAmPlayerInteractable interactable = DetectInteractables();
        Debug.Log("Interact: " + interactable);
        if (interactable != _hoveredInteractable)
        {
            ChangeHoveredInteractable(interactable);
        }
    }

    private IAmPlayerInteractable DetectInteractables()
    {
        Collider2D[] results  = new Collider2D[2];
        int count = collider.Overlap(_contactFilter, results);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = results[i];
            if (col == null) continue;

            IAmPlayerInteractable interactable = col.GetComponent<IAmPlayerInteractable>();

            if (interactable == null) continue;
            
            return interactable;
        }

        return null;
    }
    
    private void ChangeHoveredInteractable(IAmPlayerInteractable amPlayerInteractable)
    {
        _hoveredInteractable?.UnHighlight();

        _hoveredInteractable = amPlayerInteractable;
        
        if(_hoveredInteractable == null || !_hoveredInteractable.CanInteract(_playerStatusController))
            return;
        
        _hoveredInteractable.Highlight();
    }

    public void InteractClick()
    {
        Debug.Log("Interact");
        if(IsInteracting)
        {
            IsInteracting = false;
            InteractOnlyOnce = false;
        }
        else
        {
            if (_hoveredInteractable != null && _hoveredInteractable.CanInteract(_playerStatusController))
            {
                IsInteracting = true;
                InteractOnlyOnce = _hoveredInteractable.InteractOnlyOnce;
            }
            else
            {
                OnInteractFailed?.Invoke();
            }
        }
    }
    
    public void Interact()
    {
        if(!IsOwner) return;
        
        _hoveredInteractable.Interact(_playerStatusController);

        if (InteractOnlyOnce)
        {
            IsInteracting = false;
            InteractOnlyOnce = true;
        }
    }
}