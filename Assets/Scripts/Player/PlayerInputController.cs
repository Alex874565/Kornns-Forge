using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInputController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CursorVisibilityManager cursorVisibilityManager;
    
    private InputSystem_Actions controls;

    private PauseMenuUI pauseMenu;

    public event Action<Vector2> OnMove;
    public event Action OnJumpPressed;
    public event Action OnJumpReleased;
    public event Action OnInteract;
    public event Action OnCancel;
    public event Action OnInteractAlternate;
    public event Action OnInteractAlternateUI;
    public event Action OnThrow;
    public event Action OnEscape;

    public Vector2 Movement => controls.Player.Move.ReadValue<Vector2>();
    
    private void Awake()
    {
        controls = new InputSystem_Actions();
        pauseMenu  = FindFirstObjectByType<PauseMenuUI>()?.GetComponent<PauseMenuUI>();
        pauseMenu.Controls = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;
        controls.UI.Submit.performed += ctx =>
        {
            Debug.Log("UI Submit key: " + ctx.control.path);
        };
        controls.Player.Move.performed += HandleMove;
        controls.Player.Jump.performed += HandleJumpPressed;
        controls.Player.Jump.canceled += HandleJumpReleased;
        controls.Player.Interact.performed += HandleInteract;
        controls.Player.InteractAlternate.performed += HandleInteractAlternate;
        controls.Player.Throw.performed += HandleThrow;
        controls.Player.Escape.performed += HandleEscape;
        
        controls.UI.Cancel.performed += HandleCancel;
        controls.UI.InteractAlternate.performed += HandleInteractAlternateUI;

        SetUIMode(false);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;

        controls.Player.Move.performed -= HandleMove;
        controls.Player.Jump.performed -= HandleJumpPressed;
        controls.Player.Jump.canceled -= HandleJumpReleased;
        controls.Player.Interact.performed -= HandleInteract;
        controls.Player.InteractAlternate.performed -= HandleInteractAlternate;
        controls.Player.Throw.performed -= HandleThrow;
        controls.Player.Escape.performed -= HandleEscape;
        
        controls.UI.Cancel.performed -= HandleCancel;
        controls.UI.InteractAlternate.performed -= HandleInteractAlternateUI;

        controls.Player.Disable();
    }
    
    public void SetUIMode(bool uiMode, GameObject firstSelected = null)
    {
        if (!IsOwner) return;

        if (cursorVisibilityManager == null)
            cursorVisibilityManager = FindObjectOfType<CursorVisibilityManager>();

        StopAllCoroutines();
        StartCoroutine(SetUIModeCoroutine(uiMode, firstSelected));
    }

    private IEnumerator SetUIModeCoroutine(bool uiMode, GameObject firstSelected)
    {
        if (uiMode)
        {
            EventSystem.current.SetSelectedGameObject(null);
            controls.Player.Disable();
            controls.UI.Enable();
            cursorVisibilityManager.CursorVisibilityPossible = true;
            yield return !controls.UI.Submit.IsPressed() && !controls.UI.Cancel.IsPressed();
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }
        else
        {
            controls.UI.Disable();
            controls.Player.Enable();
            cursorVisibilityManager.SetCursorVisibility(false);
            cursorVisibilityManager.CursorVisibilityPossible = false;
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // Enable or disable the input action map at runtime.
    public void SetActive(bool active)
    {
        if (!IsOwner) return;

        if (active)
            controls.Player.Enable();
        else
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
        OnInteract?.Invoke();
    }

    private void HandleCancel(InputAction.CallbackContext ctx)
    {
        OnCancel?.Invoke();
    }

    private void HandleInteractAlternate(InputAction.CallbackContext ctx)
    {
        OnInteractAlternate?.Invoke();
    }
    
    private void HandleInteractAlternateUI(InputAction.CallbackContext ctx)
    {
        OnInteractAlternateUI?.Invoke();
    }

    private void HandleThrow(InputAction.CallbackContext ctx)
    {
        OnThrow?.Invoke();
    }

    private void HandleEscape(InputAction.CallbackContext ctx)
    {
        if(pauseMenu)
            pauseMenu.TogglePause();
    }
}