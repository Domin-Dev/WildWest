using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;

[DisallowMultipleComponent]
public class LocalizeTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private LocalizedString content;
    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipSystem.Show(content.GetLocalizedString());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TooltipSystem.Hide();
    }
}
