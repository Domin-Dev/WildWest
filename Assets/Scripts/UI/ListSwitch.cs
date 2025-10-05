using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class ListSwitch : BaseSwitch
{
    [SerializeField] private TextMeshProUGUI text;
    string[] tab;
    public void SetUpSwitch(string[] tab, int defaultValue)
    {
        this.tab = tab;
        this.value = defaultValue;
        ChangSwitchText(null, value);
        base.SetUpSwitch(0, tab.Length);
    }

    public void RefreshTab(string[] tab)
    {
        this.tab = tab;
        this.value = Math.Clamp(value, 0, this.tab.Length);
        ChangSwitchText(null, value);
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
        Debug.Log("new!!!" + tab[e]);
     
        if(tab != null)
            text.text = tab[e];
        Debug.Log(text.text);
    }
}
