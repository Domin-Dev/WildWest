using TMPro;
using UnityEngine;

public class CustomInputField : TMP_InputField
{
    public bool DisableInputControls = true;
    public void OnGUI()
    {
        if (Event.current.isKey && (Event.current.keyCode == KeyCode.DownArrow || Event.current.keyCode == KeyCode.UpArrow))
            Event.current.Use();
    }
}