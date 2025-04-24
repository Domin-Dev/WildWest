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
    [SerializeField] private string nameSwitch;

    public void SetUpSwitch(int minValue, int maxValue,string name)
    {
        nameSwitch = name;
        ChangSwitchText(null, value);
        base.SetUpSwitch(minValue, maxValue);
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

        OnChangedValue += ChangSwitchText;
    }

    private void ChangSwitchText(object sender, int e)
    {
        text.text = nameSwitch + " " + e.ToString();
    }
}
