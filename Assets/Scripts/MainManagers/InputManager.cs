using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static NewInput input;
    public static InputManager i;

    public InputActionAsset action;

    private InputAction equipment;
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

        equipment = action.FindActionMap("Player").FindAction("Equipment");
        equipment.performed += Equipment_performed;


    }

    private void Equipment_performed(InputAction.CallbackContext obj)
    {
        Debug.Log("dzial!!");
    }

    private void Update()
    {
        // if(input.Player.Equipment.triggered)
        //{
        //    Debug.Log("action!!!");
        //    foreach (var binding in input.Player.Equipment.)
        //    {
        //        Debug.Log($"[{binding.action}] {binding.path} (override: {binding.overridePath})");
        //    }
        //}
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