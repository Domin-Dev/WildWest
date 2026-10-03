using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;


[DisallowMultipleComponent]
public abstract class TooltipTriggerBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{   
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        Tooltip(eventData);
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Hide();
    }
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        TooltipSystem.Hide();
    }

    public abstract void Tooltip(PointerEventData eventData);
}
