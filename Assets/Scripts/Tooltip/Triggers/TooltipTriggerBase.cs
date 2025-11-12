using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;


[DisallowMultipleComponent]
public abstract class TooltipTriggerBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{   
    public void OnPointerEnter(PointerEventData eventData)
    {
        Tooltip(eventData);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Hide();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        TooltipSystem.Hide();
    }

    public abstract void Tooltip(PointerEventData eventData);
}
