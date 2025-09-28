using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static NewInput input;

    private void Awake()
    {
        if (input == null)
        {
            input = new NewInput();
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