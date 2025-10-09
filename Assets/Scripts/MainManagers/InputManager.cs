using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static NewInput input;
    public static InputManager i { private set; get; }

    private InputActionAsset action;

    private InputAction equipment;
    private InputAction move;


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
    public void SetUp(InputActionAsset inputAsset)
    {
        action = inputAsset;

        equipment = action.FindActionMap("Player").FindAction("Equipment");
        move = action.FindActionMap("Player").FindAction("Move");
        equipment.performed += Equipment_performed;
        move.performed += Move_performed;
    }
    private void Move_performed(InputAction.CallbackContext obj)
    {
        Debug.Log("Move!!");
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