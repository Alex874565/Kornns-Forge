using Unity.Netcode;
using UnityEngine.InputSystem;
using System;
using UnityEngine;

public class PlayerInputController : NetworkBehaviour
{
    private PlayerInput PlayerInput { get; set; }

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _interactAction;
    private InputAction _escapeAction;

    public event Action<Vector2> OnMove;
    public event Action OnJumpPressed, OnJumpReleased, OnInteract, OnEscape;
    
    public Vector2 Movement => _moveAction.ReadValue<Vector2>();
    
    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
        _moveAction = PlayerInput.actions["Move"];
        _jumpAction = PlayerInput.actions["Jump"];
        _interactAction = PlayerInput.actions["Interact"];
        _escapeAction = PlayerInput.actions["Escape"];
    }
    
    public override void OnNetworkSpawn()
    {
        GetComponent<PlayerInput>().enabled = IsOwner;
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _jumpAction.Enable();
        _interactAction.Enable();
        _escapeAction.Enable();
        _moveAction.performed += MoveAction;
        _jumpAction.performed += JumpPressedAction;
        _jumpAction.canceled += JumpReleasedAction;
        _interactAction.performed += InteractAction;
        _escapeAction.performed += EscapeAction;
    }

    private void MoveAction(InputAction.CallbackContext context)
    {
        OnMove?.Invoke(Movement);
    }
    
    private void JumpPressedAction(InputAction.CallbackContext context)
    {
        OnJumpPressed?.Invoke();
    }

    private void JumpReleasedAction(InputAction.CallbackContext context)
    {
        OnJumpReleased?.Invoke();
    }

    private void InteractAction(InputAction.CallbackContext context)
    {
        OnInteract?.Invoke();
    }

    private void EscapeAction(InputAction.CallbackContext context)
    {
        OnEscape?.Invoke();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _jumpAction.Disable();
        _interactAction.Disable();
        _escapeAction.Disable();
        _moveAction.performed -= MoveAction;
        _jumpAction.performed -= JumpPressedAction;
        _jumpAction.canceled -= JumpReleasedAction;
        _interactAction.performed -= InteractAction;
        _escapeAction.performed -= EscapeAction;
    }
}