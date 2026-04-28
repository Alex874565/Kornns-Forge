using Unity.Netcode;
using UnityEngine;
using System;

[RequireComponent(typeof(PlayerInputController), typeof(PlayerStatusController))]
public class PlayerInteractionController : NetworkBehaviour
{
    [SerializeField] private LayerMask interactLayerMask;
    [SerializeField] private new Collider2D collider;

    public Action OnBeginInteraction, OnEndInteraction, OnInteract, OnInteractFailed;
    
    private IPlayerInteractable _hoveredInteractable;
    
    private PlayerInputController _playerInputController;
    private PlayerStatusController _playerStatusController;
    
    private ContactFilter2D _contactFilter;
    
    public bool IsInteracting { get; set; } = false;
    //public bool InteractOnlyOnce { get; set; } = true;

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
        
        IPlayerInteractable interactable = DetectInteractables();
        if (interactable != _hoveredInteractable)
        {
            ChangeHoveredInteractable(interactable);
        }
    }

    private IPlayerInteractable DetectInteractables()
    {
        Collider2D[] results  = new Collider2D[2];
        int count = collider.Overlap(_contactFilter, results);

        for (int i = 0; i < count; i++)
        {
            Collider2D col = results[i];
            if (col == null) continue;

            IPlayerInteractable interactable = col.GetComponent<IPlayerInteractable>();

            if (interactable == null) continue;
            
            return interactable;
        }

        return null;
    }
    
    private void ChangeHoveredInteractable(IPlayerInteractable playerInteractable)
    {
        _hoveredInteractable?.UnHighlight();

        _hoveredInteractable = playerInteractable;
        
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
            //InteractOnlyOnce = false;
        }
        else
        {
            if (_hoveredInteractable != null && _hoveredInteractable.CanInteract(_playerStatusController))
            {
                IsInteracting = true;
                //InteractOnlyOnce = _hoveredInteractable.InteractOnlyOnce;
                _hoveredInteractable.Interact(_playerStatusController);
                IsInteracting = false;
            }
            else
            {
                OnInteractFailed?.Invoke();
            }
        }
    }
    
    public void Interact()
    {
        Debug.Log(IsOwner);
        if(!IsOwner) return;
        
        _hoveredInteractable.Interact(_playerStatusController);

        //if (InteractOnlyOnce)
        //{
        //    IsInteracting = false;
        //    InteractOnlyOnce = true;
        //}
    }
}