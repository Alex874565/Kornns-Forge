using Unity.Netcode;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class PlayerInputController : NetworkBehaviour
{
    private InputSystem_Actions controls;

    public event Action<Vector2> OnMove;
    public event Action OnJumpPressed;
    public event Action OnJumpReleased;
    public event Action OnInteract;
    public event Action OnEscape;

    public Vector2 Movement => controls.Player.Move.ReadValue<Vector2>();

    private void Awake()
    {
        controls = new InputSystem_Actions();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        controls.Player.Move.performed += HandleMove;
        controls.Player.Jump.performed += HandleJumpPressed;
        controls.Player.Jump.canceled += HandleJumpReleased;
        controls.Player.Interact.performed += HandleInteract;
        controls.Player.Escape.performed += HandleEscape;

        controls.Player.Enable();
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;

        controls.Player.Move.performed -= HandleMove;
        controls.Player.Jump.performed -= HandleJumpPressed;
        controls.Player.Jump.canceled -= HandleJumpReleased;
        controls.Player.Interact.started -= HandleInteract;
        controls.Player.Escape.performed -= HandleEscape;

        controls.Player.Disable();
    }

    private void HandleMove(InputAction.CallbackContext ctx)
    {
        OnMove?.Invoke(ctx.ReadValue<Vector2>());
    }

    private void HandleJumpPressed(InputAction.CallbackContext ctx)
    {
        OnJumpPressed?.Invoke();
    }

    private void HandleJumpReleased(InputAction.CallbackContext ctx)
    {
        OnJumpReleased?.Invoke();
    }

    private void HandleInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log("Interact fired");
        OnInteract?.Invoke();
    }

    private void HandleEscape(InputAction.CallbackContext ctx)
    {
        OnEscape?.Invoke();
    }
}