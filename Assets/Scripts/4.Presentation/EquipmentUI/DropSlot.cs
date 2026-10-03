using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UIElements;
using System.Security.Cryptography;
using System;

public class DropSlot : MonoBehaviour, IPointerDownHandler, IGetSlotPosition
{
    private SlotPosition slotPosition;
    public event Action<PointerEventData> slotClick;
    public void SetSlotPosition(int slotIndex, int gridIndex)
    {
        slotPosition = new SlotPosition(gridIndex, slotIndex);
    }
    public SlotPosition GetSlotPosition()
    {
        return slotPosition;
    }
    private void OnDestroy()
    {
        slotClick = null;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (slotClick != null)
            slotClick.Invoke(eventData);
        else
            DragManager.instance.SlotSelected(this, eventData);
    }
}
