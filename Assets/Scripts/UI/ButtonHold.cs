using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

public class ButtonHold : Button
{
    public bool isPressed;
    public Action action;
    Timer timer;


    public void Cancel()
    {
        isPressed = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable(); 
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        if (InputManager.input != null)
        {
            InputManager.input.UI.Submit.started -= ButtonIsPressed;
            InputManager.input.UI.Submit.canceled -= Release;
        }
    }


    int i = 0;
    int k = 0;
    public void FixedUpdate()
    {
        if (isPressed)
        {
            i++;
            k++;
            if (i >= (5 - k / 20) + 1)
            {
                action();
                i = 0;
            }
        }
    }
    private void ButtonIsPressed(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("inpu!");
        ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerDownHandler);
      /// ButtonIsPressed();
    }
    private void ButtonIsPressed()
    {
        if (interactable)
        {
            timer = Timer.Create(0.2f, () => { isPressed = true; return false; });
            k = 0;
        }
    }



    private void Release(InputAction.CallbackContext callbackContext)
    {
        ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerUpHandler);
      //  Release();
    }
    private void Release()
    {
        isPressed = false;
        if (timer != null) timer.Cancel();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        ButtonIsPressed();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        Release();
    }
    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        InputManager.input.UI.Submit.started += ButtonIsPressed;
        InputManager.input.UI.Submit.canceled += Release;
        Debug.Log("juz!!!");
    }
    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        InputManager.input.UI.Submit.started -= ButtonIsPressed;
        InputManager.input.UI.Submit.canceled -= Release;
        Debug.Log("brak111");

        ExecuteEvents.Execute(gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerUpHandler);
        Release();
    }
}

