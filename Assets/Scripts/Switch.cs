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

public class Switch : MonoBehaviour
{
    [SerializeField] private Button left;
    [SerializeField] private Button rigth;
    [SerializeField] private TextMeshProUGUI text;
    [Space]
    [SerializeField] private int value = 0;
    [Space]
    [SerializeField] private int minValue;
    [SerializeField] private int maxValue;
    [SerializeField] private string nameSwitch;

    public event EventHandler<SwitchArgs> OnChangedValue;

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

    private void ChangSwitchText(object sender, SwitchArgs e)
    {
        text.text = nameSwitch + " " + e.newValue.ToString();
    }

    public void SetUpSwitch(int minValue,int maxValue,string nameSwitch)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.nameSwitch = nameSwitch;
    }

    private void DecreaseValue()
    {
        if(value == minValue)
        {
            value = maxValue - 1;
        }
        else
        {
            value--;
        }
        OnChangedValue?.Invoke(this, new SwitchArgs(value));
    }

    private void IncreaseValue()
    {
        if (value + 1 >= maxValue)
        {
            value = minValue;
        }
        else
        {
            value++;
        }
        OnChangedValue?.Invoke(this, new SwitchArgs(value));
    }
}
