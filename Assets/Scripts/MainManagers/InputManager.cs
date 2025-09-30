using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static NewInput input;
    public static InputManager i;

    private void Awake()
    {
        if (i == null)
        {
            input = new NewInput();
            i = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        input.UI.Enable();
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.UI.Disable();
        input.Player.Disable();
    }
}