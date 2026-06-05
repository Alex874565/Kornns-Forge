using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInputController : NetworkBehaviour
{
    [SerializeField] private PromptTexts interactPrompt;
    [SerializeField] private PromptTexts alternateInteractPrompt;
    [SerializeField] private PromptTexts cancelPrompt;
    [Header("References")]
    [SerializeField] private CursorVisibilityManager cursorVisibilityManager;

    public enum PlayerInputDeviceType
    {
        Keyboard,
        Mouse,
        Gamepad
    }
    
    [Serializable]
    public class PromptTexts
    {
        public string keyboard;
        public string gamepad;
    }

    public PlayerInputDeviceType CurrentInputDevice { get; private set; } =
        PlayerInputDeviceType.Keyboard;

    public event Action<PlayerInputDeviceType> OnInputDeviceChanged;

    private InputSystem_Actions controls;
    private PauseMenuUI pauseMenu;
    private Coroutine uiModeCoroutine;

    private Vector2 lastMousePosition;

    public event Action<Vector2> OnMove;
    public event Action OnJumpPressed;
    public event Action OnJumpReleased;
    public event Action OnInteract;
    public event Action OnCancel;
    public event Action OnInteractAlternate;
    public event Action OnInteractAlternateUI;
    public event Action OnThrow;

    public Vector2 Movement =>
        controls != null && controls.Player.enabled
            ? controls.Player.Move.ReadValue<Vector2>()
            : Vector2.zero;

    private void Awake()
    {
        controls = new InputSystem_Actions();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            controls.Player.Disable();
            controls.UI.Disable();
            enabled = false;
            return;
        }

        cursorVisibilityManager = FindFirstObjectByType<CursorVisibilityManager>();

        pauseMenu = FindFirstObjectByType<PauseMenuUI>(FindObjectsInactive.Include);
        if (pauseMenu != null)
            pauseMenu.Controls = this;

        GameOverUI gameOverMenu = FindFirstObjectByType<GameOverUI>(FindObjectsInactive.Include);
        if (gameOverMenu != null)
            gameOverMenu.Controls = this;

        if (Mouse.current != null)
            lastMousePosition = Mouse.current.position.ReadValue();

        SubscribeInput();
        SetUIMode(false);
    }

    public override void OnNetworkDespawn()
    {
        UnsubscribeInput();

        controls.Player.Disable();
        controls.UI.Disable();

        if (uiModeCoroutine != null)
        {
            StopCoroutine(uiModeCoroutine);
            uiModeCoroutine = null;
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        DetectMouseMovement();
    }

    private void SubscribeInput()
    {
        controls.Player.Move.performed += DetectInputDevice;
        controls.Player.Jump.performed += DetectInputDevice;
        controls.Player.Interact.performed += DetectInputDevice;
        controls.Player.InteractAlternate.performed += DetectInputDevice;
        controls.Player.Throw.performed += DetectInputDevice;
        controls.Player.Escape.performed += DetectInputDevice;

        controls.UI.Submit.performed += DetectInputDevice;
        controls.UI.Cancel.performed += DetectInputDevice;
        controls.UI.InteractAlternate.performed += DetectInputDevice;
        controls.UI.Navigate.performed += DetectInputDevice;

        controls.Player.Move.performed += HandleMove;
        controls.Player.Jump.performed += HandleJumpPressed;
        controls.Player.Jump.canceled += HandleJumpReleased;
        controls.Player.Interact.performed += HandleInteract;
        controls.Player.InteractAlternate.performed += HandleInteractAlternate;
        controls.Player.Throw.performed += HandleThrow;
        controls.Player.Escape.performed += HandleEscape;

        controls.UI.Cancel.performed += HandleCancel;
        controls.UI.InteractAlternate.performed += HandleInteractAlternateUI;
    }

    private void UnsubscribeInput()
    {
        if (controls == null)
            return;

        controls.Player.Move.performed -= DetectInputDevice;
        controls.Player.Jump.performed -= DetectInputDevice;
        controls.Player.Interact.performed -= DetectInputDevice;
        controls.Player.InteractAlternate.performed -= DetectInputDevice;
        controls.Player.Throw.performed -= DetectInputDevice;
        controls.Player.Escape.performed -= DetectInputDevice;

        controls.UI.Submit.performed -= DetectInputDevice;
        controls.UI.Cancel.performed -= DetectInputDevice;
        controls.UI.InteractAlternate.performed -= DetectInputDevice;
        controls.UI.Navigate.performed -= DetectInputDevice;

        controls.Player.Move.performed -= HandleMove;
        controls.Player.Jump.performed -= HandleJumpPressed;
        controls.Player.Jump.canceled -= HandleJumpReleased;
        controls.Player.Interact.performed -= HandleInteract;
        controls.Player.InteractAlternate.performed -= HandleInteractAlternate;
        controls.Player.Throw.performed -= HandleThrow;
        controls.Player.Escape.performed -= HandleEscape;

        controls.UI.Cancel.performed -= HandleCancel;
        controls.UI.InteractAlternate.performed -= HandleInteractAlternateUI;
    }

    private void DetectMouseMovement()
    {
        if (Mouse.current == null)
            return;

        Vector2 currentMousePosition = Mouse.current.position.ReadValue();

        if (currentMousePosition == lastMousePosition)
            return;

        lastMousePosition = currentMousePosition;

        SetInputDevice(PlayerInputDeviceType.Mouse);
    }

    private void DetectInputDevice(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device is Gamepad)
        {
            SetInputDevice(PlayerInputDeviceType.Gamepad);
        }
        else if (ctx.control.device is Mouse)
        {
            SetInputDevice(PlayerInputDeviceType.Mouse);
        }
        else
        {
            SetInputDevice(PlayerInputDeviceType.Keyboard);
        }
    }

    private void SetInputDevice(PlayerInputDeviceType device)
    {
        Debug.Log("Input device: " + device);
        if (cursorVisibilityManager != null)
            cursorVisibilityManager.HandleInputDeviceChanged(device);

        if (device == CurrentInputDevice)
            return;

        CurrentInputDevice = device;
        OnInputDeviceChanged?.Invoke(CurrentInputDevice);
    }

    public void SetUIMode(bool uiMode, GameObject firstSelected = null)
    {
        if (!IsOwner || controls == null)
            return;

        if (cursorVisibilityManager == null)
            cursorVisibilityManager = FindFirstObjectByType<CursorVisibilityManager>();

        if (uiModeCoroutine != null)
            StopCoroutine(uiModeCoroutine);

        uiModeCoroutine = StartCoroutine(SetUIModeCoroutine(uiMode, firstSelected));
    }

    private IEnumerator SetUIModeCoroutine(bool uiMode, GameObject firstSelected)
    {
        controls.Player.Disable();
        controls.UI.Disable();

        if (uiMode)
        {
            controls.UI.Enable();

            if (cursorVisibilityManager != null)
            {
                cursorVisibilityManager.CursorVisibilityPossible = true;
                cursorVisibilityManager.HandleInputDeviceChanged(CurrentInputDevice);
            }

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            yield return new WaitUntil(() =>
                !controls.UI.Submit.IsPressed() &&
                !controls.UI.Cancel.IsPressed()
            );

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(firstSelected);
        }
        else
        {
            controls.Player.Enable();

            if (cursorVisibilityManager != null)
            {
                cursorVisibilityManager.CursorVisibilityPossible = false;
                cursorVisibilityManager.SetCursorVisibility(false);
            }

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        uiModeCoroutine = null;
    }

    public void SetActive(bool active)
    {
        if (!IsOwner || controls == null)
            return;

        if (uiModeCoroutine != null)
        {
            StopCoroutine(uiModeCoroutine);
            uiModeCoroutine = null;
        }

        controls.UI.Disable();

        if (active)
            controls.Player.Enable();
        else
            controls.Player.Disable();
    }

    public bool IsUsingMouse()
    {
        return CurrentInputDevice == PlayerInputDeviceType.Mouse;
    }

    private string GetPrompt(PromptTexts prompt)
    {
        switch (CurrentInputDevice)
        {
            case PlayerInputDeviceType.Mouse:
                return "";

            case PlayerInputDeviceType.Gamepad:
                return prompt.gamepad;

            default:
                return prompt.keyboard;
        }
    }
    
    public string GetInteractPrompt()
    {
        return GetPrompt(interactPrompt);
    }

    public string GetAlternateInteractPrompt()
    {
        return GetPrompt(alternateInteractPrompt);
    }

    public string GetCancelPrompt()
    {
        return GetPrompt(cancelPrompt);
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
        if (pauseMenu != null)
            pauseMenu.TogglePause();
    }

    private void OnDestroy()
    {
        controls?.Dispose();
    }
}