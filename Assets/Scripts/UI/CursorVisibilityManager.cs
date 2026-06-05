using UnityEngine;
using UnityEngine.InputSystem;

public class CursorVisibilityManager : MonoBehaviour
{
    [field: SerializeField] public bool CursorVisibilityPossible { get; set; } = true;

    public PlayerInputController.PlayerInputDeviceType CurrentInputDevice { get; private set; } =
        PlayerInputController.PlayerInputDeviceType.Keyboard;

    private Vector2 lastMousePosition;
    [SerializeField] private float mouseMoveThreshold = 0.1f;

    private void Start()
    {
        if (Mouse.current != null)
            lastMousePosition = Mouse.current.position.ReadValue();

        SetCursorVisibility(false);
    }

    private void Update()
    {
        DetectMouseMovement();
        DetectKeyboardOrGamepad();
    }

    private void DetectMouseMovement()
    {
        if (Mouse.current == null)
            return;

        Vector2 currentMousePosition = Mouse.current.position.ReadValue();

        if (Vector2.Distance(currentMousePosition, lastMousePosition) < mouseMoveThreshold)
            return;

        lastMousePosition = currentMousePosition;

        SetInputDevice(PlayerInputController.PlayerInputDeviceType.Mouse);
    }

    private void DetectKeyboardOrGamepad()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SetInputDevice(PlayerInputController.PlayerInputDeviceType.Keyboard);
            return;
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame ||
                Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.1f ||
                Gamepad.current.dpad.ReadValue().sqrMagnitude > 0.1f)
            {
                SetInputDevice(PlayerInputController.PlayerInputDeviceType.Gamepad);
            }
        }
    }

    public void HandleInputDeviceChanged(PlayerInputController.PlayerInputDeviceType device)
    {
        SetInputDevice(device);
    }

    private void SetInputDevice(PlayerInputController.PlayerInputDeviceType device)
    {
        CurrentInputDevice = device;

        bool showCursor =
            CursorVisibilityPossible &&
            device == PlayerInputController.PlayerInputDeviceType.Mouse;

        SetCursorVisibility(showCursor);
    }

    public void SetCursorVisibility(bool visible)
    {
        Cursor.visible = visible;
    }
}