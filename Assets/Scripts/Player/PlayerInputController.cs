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

        controls.Player.Enable();

        controls.Player.Move.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        controls.Player.Jump.performed += ctx => OnJumpPressed?.Invoke();
        controls.Player.Jump.canceled += ctx => OnJumpReleased?.Invoke();
        controls.Player.Interact.performed += ctx => OnInteract?.Invoke();
        controls.Player.Escape.performed += ctx => OnEscape?.Invoke();
    }

    private void OnDisable()
    {
        if (controls != null)
            controls.Player.Disable();
    }
}