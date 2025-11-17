using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[DisallowMultipleComponent]
public class StaticTooltipTrigger:  TooltipTriggerBase
{
    private IHaveTooltip content;
    public void SetUp(IHaveTooltip content)
    {
        this.content = content;
    }
    public override void Tooltip(PointerEventData eventData)
    {
        TooltipSystem.Show(content);
    }
}
