using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

[DisallowMultipleComponent]
public class DynamicTooltipTrigger:  TooltipTriggerBase
{
    private Func<TooltipInfo> getContent;
    public void SetUp(Func<TooltipInfo> func)
    {
        this.getContent = func;
    }
    public override void Tooltip(PointerEventData eventData)
    {
        if(getContent != null)
            TooltipSystem.Show(getContent.Invoke());
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        TooltipSystem.AddTriggerToUpdate(this);
        base.OnPointerEnter(eventData);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.RemoveTriggerToUpdate(this);
        base.OnPointerExit(eventData);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        TooltipSystem.RemoveTriggerToUpdate(this);
        base.OnPointerClick(eventData);
    }

    public virtual void UpdateTooltip()
    {
        TooltipSystem.ShowInstant(getContent.Invoke());
    }
}

