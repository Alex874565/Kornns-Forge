using UnityEngine;
using UnityEngine.InputSystem;

public class CursorVisibilityManager : MonoBehaviour
{
    [field: SerializeField] public bool CursorVisibilityPossible { get; set; }
    
    private void Start()
    {
        SetCursorVisibility(false);
        InputSystem.onActionChange += OnInputActionChange;
    }

    private void OnInputActionChange(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionPerformed)
        {
            InputAction inputAction = (InputAction) obj;
            InputControl inputControl = inputAction.activeControl;
            InputDevice inputDevice = inputControl.device;
            
            SetCursorVisibility(CursorVisibilityPossible && inputDevice is Mouse);
        }
    }
    
    public void SetCursorVisibility(bool cursorVisibility)
    {
        Cursor.visible = cursorVisibility;
    }
}