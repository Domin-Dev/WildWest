using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        if(tab != null)
            text.text = tab[e];
    }
}
