using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

[DisallowMultipleComponent]
public class LocalizeTooltipTrigger :  TooltipTriggerBase
{
    [SerializeField] private LocalizedString content;
    public override void Tooltip(PointerEventData eventData)
    {
        TooltipSystem.Show(content.GetLocalizedString());
    }
}
