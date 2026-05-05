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
        
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;
        _playerInputController.OnInteract -= InteractClick;
        _playerInputController.OnInteractAlternate -= InteractAlternateClick;
        _playerInputController.OnThrow -= Throw;
        
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

        IPlayerInteractable itemFallback = null;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = results[i];
            //Debug.Log("Checking: " + col.name);
            if (col == null) continue;

            IPlayerInteractable interactable = col.GetComponentInParent<IPlayerInteractable>();
            if (interactable == null) continue;

            // ✅ PRIORITY: stations first
            if (interactable is BaseStation)
            {
                return interactable;
            }

            // 🟡 fallback: item
            if (itemFallback == null)
            {
                itemFallback = interactable;
            }
        }

        return itemFallback;
    }
    
    private void ChangeHoveredInteractable(IPlayerInteractable playerInteractable)
    {
        Debug.Log("Hovered: " + playerInteractable);
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
        Debug.Log("Hovered interactable = " + _hoveredInteractable);
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
                if (_hoveredInteractable is BaseStation station && station.NetworkObject != null)
                {
                    InteractServerRpc(station.NetworkObjectId);
                }
                IsInteracting = false;
            }
            else
            {
                OnInteractFailed?.Invoke();
            }
        }
    }

    public void InteractAlternateClick()
    {
        Debug.Log("ALT CLICK");

        if (_hoveredInteractable is BaseStation station && station.NetworkObject != null)
        {
            Debug.Log("Sending ALT RPC to: " + station.name);
            InteractAlternateServerRpc(station.NetworkObjectId);
        }
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

    [ServerRpc]
    private void InteractServerRpc(ulong stationId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
            .TryGetValue(stationId, out NetworkObject netObj))
            return;

        BaseStation station = netObj.GetComponent<BaseStation>();
        if (station == null) return;

        var playerObj = NetworkManager.Singleton.SpawnManager
            .GetPlayerNetworkObject(OwnerClientId);

        if (playerObj == null) return;

        PlayerStatusController player = playerObj.GetComponent<PlayerStatusController>();
        if (player == null) return;

        station.Interact(player);
    }

    [ServerRpc]
    private void InteractAlternateServerRpc(ulong stationId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
            .TryGetValue(stationId, out NetworkObject netObj))
            return;

        BaseStation station = netObj.GetComponent<BaseStation>();
        if (station == null) return;

        var playerObj = NetworkManager.Singleton.SpawnManager
            .GetPlayerNetworkObject(OwnerClientId);
        if (playerObj == null) return;

        PlayerStatusController player = playerObj.GetComponent<PlayerStatusController>();
        if (player == null) return;

        station.InteractAlternate(player);
    }
}