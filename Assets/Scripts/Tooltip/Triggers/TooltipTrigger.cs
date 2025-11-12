using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;


[DisallowMultipleComponent]
public class TooltipTrigger : TooltipTriggerBase
{
    [SerializeField] private string header;
    [Multiline()]
    [SerializeField] private string content;

    public override void Tooltip(PointerEventData eventData)
    {
        TooltipSystem.Show(header, content);
    }
}
