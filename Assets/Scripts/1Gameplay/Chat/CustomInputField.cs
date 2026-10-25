using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomInputField : TMP_InputField
{
    public bool DisableInputControls = true;
    public event Action OnCaretPositionChanged;
    private int oldCaretPosition = -1;
    public void OnGUI()
    {
        if (Event.current.isKey && (Event.current.keyCode == KeyCode.DownArrow || Event.current.keyCode == KeyCode.UpArrow))
            Event.current.Use();
    }
    // public override void OnUpdateSelected(BaseEventData eventData)
    // {
    //     base.OnUpdateSelected(eventData);
    //     if(stringPosition != oldCaretPosition)
    //     {
    //         oldCaretPosition = stringPosition;
    //         OnCaretPositionChanged?.Invoke();
    //     }
    // }

    public override void OnMove(AxisEventData eventData)
    {
        base.OnMove(eventData);
        OnCaretPositionChanged?.Invoke();
    }
 }