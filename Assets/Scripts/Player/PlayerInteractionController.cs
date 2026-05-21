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
    private PlayerStatusController playerStatusController;
    
    private ContactFilter2D _contactFilter;
    
    public bool IsInteracting { get; set; } = false;
    //public bool InteractOnlyOnce { get; set; } = true;

    private BaseStation _selectedStation;

    private int _interactableLayer;
    private int _itemLayer;
    
    private Vector3 lastInteractDir;

    private void Awake()
    {
        playerStatusController = GetComponent<PlayerStatusController>();
    }

    public override void OnNetworkSpawn()
    {
        if(!IsOwner) return;
        _playerInputController = GetComponent<PlayerInputController>();
        
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

            if (col.gameObject.layer == _itemLayer)
            {

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
                
                if(interactable is Order order)
                {
                    return order;
                }
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
        _hoveredInteractable?.UnHighlight();

        _hoveredInteractable = playerInteractable;

        _selectedStation = playerInteractable as BaseStation;

        if (_hoveredInteractable == null ||
            !_hoveredInteractable.CanInteract(playerStatusController))
            return;

        _hoveredInteractable.Highlight();
    }

    private void InteractClick()
    {
        if (IsInteracting) return;
        if (_hoveredInteractable == null) return;

        if (!_hoveredInteractable.CanInteract(playerStatusController))
        {
            OnInteractFailed?.Invoke();
            return;
        }

        bool isBed = (_hoveredInteractable as Component).CompareTag("Bed");
        if (!isBed && playerStatusController.GetEnergyLevel() <= 0)
            return;

        Component component = _hoveredInteractable as Component;
        if (component == null) return;

        NetworkObject networkObject = component.GetComponentInParent<NetworkObject>();
        if (networkObject == null) return;

        // Stations go through server
        if (_hoveredInteractable is BaseStation)
        {
            InteractServerRpc(networkObject.NetworkObjectId);
            return;
        }

        // Items/orders can still use their own interaction logic
        _hoveredInteractable.Interact(playerStatusController);
    }

    private void InteractAlternateClick()
    {
        Debug.Log("ALT CLICK");

        if (_hoveredInteractable is Component component)
        {
            NetworkObject networkObject =
                component.GetComponentInParent<NetworkObject>();

            if (networkObject != null)
                InteractAlternateServerRpc(networkObject.NetworkObjectId);
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

    [ServerRpc(RequireOwnership = false)]
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

    [ServerRpc(RequireOwnership = false)]
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