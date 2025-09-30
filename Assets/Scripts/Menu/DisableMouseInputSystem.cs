using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class DisableMouseInputSystem : MonoBehaviour
{
    public static DisableMouseInputSystem instance { private set; get; }

    public Action onSwitchToGamepad;
    public Action onSwitchToMouse;
    public static bool mouseEnabled { get; private set; } = true;
    InputAction anyGamepadAction;
    InputAction anyMouseAction;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

    }


    void OnEnable()
    {
        anyGamepadAction = new InputAction(type: InputActionType.Button, binding: "<Gamepad>/*");
        anyGamepadAction.performed += ctx => SwitchToGamepad();
        anyGamepadAction.Enable();

        anyMouseAction = new InputAction(type: InputActionType.Button);

        anyMouseAction.AddBinding("<Mouse>/leftButton");
        anyMouseAction.AddBinding("<Mouse>/rightButton");
        anyMouseAction.AddBinding("<Mouse>/middleButton");

        anyMouseAction.performed += ctx => SwitchToMouse();
        anyMouseAction.Enable();
    }

    void OnDisable()
    {
        anyGamepadAction?.Disable();
        anyMouseAction?.Disable();
    }

    void SwitchToGamepad()
    {
        if (!mouseEnabled) return;

        WindowsManager.instance.RefreshSelection();
        mouseEnabled = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Debug.Log("➡️ Sterowanie przełączone na gamepad (mysz wyłączona)");
        onSwitchToGamepad?.Invoke();
    }

    void SwitchToMouse()
    {
        Debug.Log("➡️ Sterowanie przełączone na gamepad (mysz wyłączona)");

        if (mouseEnabled) return;

        WindowsManager.instance.ClearSelection();
        mouseEnabled = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("➡️ Sterowanie przełączone na mysz (mysz włączona)");
        onSwitchToMouse?.Invoke();
    }
}

