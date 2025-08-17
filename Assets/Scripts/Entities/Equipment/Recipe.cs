
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Recipe : MonoBehaviour, IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler
{
    public int id;
    public void SetID(int id)
    {
        this.id = id;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        UIManager.instance.Craft(id);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipInfo tooltipInfo = ItemsAsset.instance.GetTooltipInfo(id);
        TooltipSystem.Show(tooltipInfo.content, tooltipInfo.header);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Hide();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UIManager.instance.ButtonIsHolding(true,id);

    }
    public void OnPointerUp(PointerEventData eventData)
    {
        UIManager.instance.ButtonIsHolding(false,id);
    }
}
