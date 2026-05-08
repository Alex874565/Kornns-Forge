using Unity.Netcode;
using UnityEngine;
using System;
using Unity.VisualScripting;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerInputController), typeof(PlayerStatusController))]
public class PlayerInteractionController : NetworkBehaviour
{
    [SerializeField] private LayerMask interactLayerMask;
    [SerializeField] private new Collider2D interactionCollider;
    
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
        int count = interactionCollider.Overlap(_contactFilter, results);

        IPlayerInteractable stationFallback = null;

        for (int i = 0; i < count; i++)
        {
            Collider2D col = results[i];
            if (col == null) continue;

            IPlayerInteractable interactable =
                col.GetComponentInParent<IPlayerInteractable>();

            if (interactable == null) continue;

            // PRIORITY 1: item, but only if it is NOT on an Interactable layer parent
            if (interactable is Ingredient ingredient)
            {
                IIngredientParent parent = ingredient.GetIngredientParent();

                if (parent is Component parentComponent &&
                    parentComponent.gameObject.layer == _interactableLayer)
                {
                    continue;
                }

                return ingredient;
            }

            // PRIORITY 2: anything on Interactable layer
            if (col.gameObject.layer == _interactableLayer && stationFallback == null)
            {
                stationFallback = interactable;
            }
        }

        return stationFallback;
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
        if (IsInteracting)
        {
            IsInteracting = false;
            return;
        }

        if (_hoveredInteractable == null ||
            !_hoveredInteractable.CanInteract(_playerStatusController))
        {
            OnInteractFailed?.Invoke();
            return;
        }

        IsInteracting = true;

        // Client-only interaction, no RPC
        if (_hoveredInteractable is CraftingStationController craftingStation)
        {
            craftingStation.OpenUIClientOnly(_playerStatusController);
        }
        else if (_hoveredInteractable is Component component &&
                 component.gameObject.layer == _interactableLayer &&
                 component.TryGetComponent(out NetworkObject networkObject))
        {
            InteractServerRpc(networkObject.NetworkObjectId);
        }
        else
        {
            _hoveredInteractable.Interact(_playerStatusController);
        }

        IsInteracting = false;
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
        if (IsInteracting) return;

        ThrowServerRpc(transform.right);
    }
    
    [ServerRpc]
    private void ThrowServerRpc(Vector2 direction)
    {
        var playerObj = NetworkManager.Singleton.SpawnManager
            .GetPlayerNetworkObject(OwnerClientId);

        if (playerObj == null) return;

        PlayerStatusController player =
            playerObj.GetComponent<PlayerStatusController>();

        if (player == null) return;

        if (player.HasIngredient())
        {
            Ingredient ingredient = player.GetIngredient();
            if (ingredient == null) return;

            player.ClearIngredient();
            ingredient.ThrowSelf(direction, throwForce, throwAngle);
        }
        else if (player.HasOrder())
        {
            Order order = player.GetOrder();
            if (order == null) return;

            player.ClearOrder();
            order.ThrowSelf(direction, throwForce, throwAngle);
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