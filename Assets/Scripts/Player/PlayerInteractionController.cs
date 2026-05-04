using Unity.Netcode;
using UnityEngine;
using System;
using Unity.VisualScripting;

[RequireComponent(typeof(PlayerInputController), typeof(PlayerStatusController))]
public class PlayerInteractionController : NetworkBehaviour
{
    [SerializeField] private LayerMask interactLayerMask;
    [SerializeField] private new Collider2D collider;
    
    [Header("Throw")]
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwAngle = 45f;

    public Action OnBeginInteraction, OnEndInteraction, OnInteract, OnInteractFailed;
    
    private IPlayerInteractable _hoveredInteractable;
    
    private PlayerInputController _playerInputController;
    private PlayerStatusController _playerStatusController;
    
    private ContactFilter2D _contactFilter;
    
    public bool IsInteracting { get; set; } = false;
    //public bool InteractOnlyOnce { get; set; } = true;

    private BaseStation _selectedStation;

    private int _interactableLayer;
    private int _itemLayer;
    
    public override void OnNetworkSpawn()
    {
        if(!IsOwner) return;
        _playerInputController = GetComponent<PlayerInputController>();
        _playerStatusController = GetComponent<PlayerStatusController>();
        
        _playerInputController.OnInteract += InteractClick;
        _playerInputController.OnInteractAlternate += InteractAlternateClick;
        _playerInputController.OnThrow += Throw;
        
        _contactFilter = new ContactFilter2D();
        _contactFilter.SetLayerMask(interactLayerMask);
        _contactFilter.useLayerMask = true;
        _contactFilter.useTriggers = true;
        
        _interactableLayer = LayerMask.NameToLayer("Interactable");
        _itemLayer = LayerMask.NameToLayer("Item");
        
        OnInteract += Interact;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        _playerInputController.OnInteract -= InteractClick;
        _playerInputController.OnInteractAlternate -= InteractAlternateClick;
        _playerInputController.OnThrow -= Throw;
        
        OnInteract -= Interact;
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
        Collider2D[] results = new Collider2D[8];
        int count = collider.Overlap(_contactFilter, results);

        IPlayerInteractable fallback = null;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = results[i];
            if (col == null) continue;

            IPlayerInteractable interactable = col.GetComponentInParent<IPlayerInteractable>();
            if (interactable == null) continue;

            // ✅ Priority layer
            if (col.gameObject.layer == _itemLayer)
            {
                return interactable;
            }

            // 🟡 Save fallback (e.g. Item)
            if (fallback == null)
            {
                fallback = interactable;
            }
        }

        return fallback;
    }
    
    private void ChangeHoveredInteractable(IPlayerInteractable playerInteractable)
    {
        _hoveredInteractable?.UnHighlight();

        _hoveredInteractable = playerInteractable;

        // 🔥 THIS WAS MISSING
        _selectedStation = playerInteractable as BaseStation;

        if (_hoveredInteractable == null || !_hoveredInteractable.CanInteract(_playerStatusController))
            return;

        _hoveredInteractable.Highlight();
    }

    private void InteractClick()
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

    private void InteractAlternateClick()
    {
        Debug.Log("InteractAlternateClicked");
        if (_selectedStation != null )
        {
            _selectedStation.InteractAlternate(_playerStatusController);
        }
    }
    
    private void Interact()
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

    private void Throw()
    {
        if(IsInteracting) return;
        
        if(_playerStatusController.HasIngredient())
        {
            _playerStatusController.GetIngredient().ThrowSelf(transform.right, throwForce, throwAngle);
            _playerStatusController.ClearIngredient();
        }else if (_playerStatusController.HasOrder())
        {
            _playerStatusController.GetOrder().ThrowSelf(transform.right, throwForce, throwAngle);
            _playerStatusController.ClearOrder();
        }
    }
}