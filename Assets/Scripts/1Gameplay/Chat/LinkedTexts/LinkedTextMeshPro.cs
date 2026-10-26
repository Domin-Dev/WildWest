using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(LinkedText))]
public class LinkedTextMeshPro : TextMeshProUGUI
{
    public RectTransform linkedText;
    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        if(linkedText != null) linkedText.anchoredPosition = this.rectTransform.anchoredPosition;
    }
} 