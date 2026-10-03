using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class BaseSwitch : MonoBehaviour
{
    [SerializeField] protected Button left;
    [SerializeField] protected Button rigth;

    [SerializeField] protected int value = 0;
    [SerializeField] private int minValue;
    [SerializeField] private int maxValue;

    
    public Action<int> OnChangedValue;

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


    public void SetUpSwitch(int minValue, int maxValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.value = minValue;
    }

    protected void DecreaseValue()
    {
        if (value == minValue)
        {
            value = maxValue - 1;
        }
        else
        {
            value--;
        }
        OnChangedValue?.Invoke(value);
    }

    protected void IncreaseValue()
    {
        if (value + 1 >= maxValue)
        {
            value = minValue;
        }
        else
        {
            value++;
        }
        OnChangedValue?.Invoke(value);
    }

    public int GetValue()
    {
        return value;
    }
}