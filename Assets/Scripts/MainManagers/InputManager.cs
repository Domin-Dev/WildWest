using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static NewInput input;
    public static InputManager i { private set; get; }

    private InputActionAsset action;





    public InputAction moveAllTheItems { private set; get; }
    public InputAction moveTheItem { private set; get; }


    public InputAction playerList { private set; get; }
    public InputAction chat { private set; get; }
    public InputAction equipment { private set; get; }


    public InputAction move { private set; get; }
    public InputAction sideAction { private set; get; }
    public InputAction mainAction { private set; get; }

    
    
    public InputAction previousSlot { private set; get; }
    public InputAction nextSlot { private set; get; }


    public InputAction previousAmmo { private set; get; }
    public InputAction nextAmmo { private set; get; }


    public List<InputAction> slots { private set; get; }



    public InputAction debugStats { private set; get; }


    public bool playerTab;

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

        moveAllTheItems = action.FindAction("MoveAllTheItems");
        moveTheItem = action.FindAction("MoveTheItem");  

        playerList = action.FindAction("PlayerList");
        chat = action.FindAction("Chat");
        equipment = action.FindAction("Equipment");


        move = action.FindAction("Move");
        mainAction = action.FindAction("MainAction");
        sideAction = action.FindAction("SideAction");

        previousSlot = action.FindAction("PreviousSlot");
        nextSlot = action.FindAction("NextSlot");

        previousAmmo = action.FindAction("PreviousAmmo");
        nextAmmo = action.FindAction("NextAmmo");

        slots = new List<InputAction>();
        for (int i = 1; i < 10; i++)
        {
            slots.Add(action.FindAction($"Slot{i}"));
        }
        slots.Add(action.FindAction($"Slot0"));

        debugStats = action.FindAction("DebugStats");


        previousSlot.performed += PreviousSlot_performed;
        moveTheItem.performed += PlayerList_performed;

        move.performed += Move_performed;
    }

    public int GetNextSlotInHand(int currentSlot)
    {
        for (int i = 0; i < 10; i++)
        {
            if(slots[i].triggered)
                return i;
        }

        if(previousSlot.triggered)
            return (currentSlot - 1 + 10) % 10;

        if(nextSlot.triggered)
            return (currentSlot + 1) % 10;   

        return math.clamp(currentSlot,0,9);
    }

    public int GetNextAmmoIndex(int currentIndex)
    {
        if(nextAmmo.triggered)
            return (currentIndex + 1) % int.MaxValue;   

        return currentIndex;
    }


    private void PreviousSlot_performed(InputAction.CallbackContext obj)
    {

    }

    private void PlayerList_performed(InputAction.CallbackContext obj)
    {
        playerTab = true;
    }


    private void Move_performed(InputAction.CallbackContext obj)
    {

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