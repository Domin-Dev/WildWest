using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UIElements;
using System.Security.Cryptography;

public class DropSlot : MonoBehaviour, IDropHandler, IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
{
    private SlotPosition slotPosition;

    public void SetSlotPosition(int slotIndex, int gridIndex)
    {
        slotPosition = new SlotPosition(gridIndex, slotIndex);
    }
    public SlotPosition GetSlotPosition()
    {
        return slotPosition;
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (EquipmentManager.instance.input == eventData.button)
        {

            DragDrop dragDrop = null;
            if (eventData.pointerDrag != null && eventData.pointerDrag.TryGetComponent(out dragDrop))
            {
                EquipmentManager.instance.MoveSelectedItem(slotPosition);
                eventData.pointerDrag.transform.SetParent(transform);
                eventData.pointerDrag.transform.SetAsFirstSibling();
                eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                dragDrop.IsInSlot();
                Sounds.instance.Shield();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipInfo tooltipInfo = EquipmentManager.instance.GetTooltipInfo(slotPosition);
        if(tooltipInfo != null)
        {
            TooltipSystem.Show(tooltipInfo.content,tooltipInfo.header);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Hide();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Middle)
        {
            if (EquipmentManager.instance.IsNotSelected())
            {
                if (!EquipmentManager.instance.IsFreeSlot(slotPosition))
                {
                    if (eventData.clickCount > 1)
                    {
                        Sounds.instance.Shield();
                        eventData.clickCount = 0;
                        EquipmentManager.instance.CollectAll(slotPosition);
                    }
                    else if (Input.GetKey(KeyCode.LeftControl))
                    {
                        EquipmentManager.instance.MoveUpItem(slotPosition);
                        Sounds.instance.Shield();
                    }
                    else if(Input.GetKey(KeyCode.LeftShift))
                    {
                        EquipmentManager.instance.MoveUpItems(slotPosition);
                        Sounds.instance.Shield();
                    }
                }
            }
            else
            {
                EquipmentManager.instance.PutOneItem(slotPosition);
                Sounds.instance.Shield();
            }            
        }
    }





}
