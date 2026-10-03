using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SwitchArgs : EventArgs
{
    public int newValue;
    public SwitchArgs(int newValue)
    {
        this.newValue = newValue;
    }
}

public class Switch : BaseSwitch
{
    [SerializeField] private TextMeshProUGUI text;
    public string nameSwitch;

    private bool printIndex = true;
    private bool updateText = true;

    public void SetUpSwitch(int minValue, int maxValue,string name, bool printIndex = true, bool updateText = true)
    {
        this.printIndex = printIndex;   
        nameSwitch = name;
        
        base.SetUpSwitch(minValue, maxValue);

        if (updateText)
        {
            OnChangedValue += ChangSwitchText;
            ChangSwitchText(minValue);
        }
    }

    private void Start()
    {
        left.onClick.AddListener(() =>
        {
            DecreaseValue();
        });

        rigth.onClick.AddListener(() =>
        {
            IncreaseValue();
        });
    }

    private void ChangSwitchText(int e)
    {
        text.text = (nameSwitch.Length == 0 ? (nameSwitch + " ") : "") + (printIndex ? e.ToString() : "");
    }
}
