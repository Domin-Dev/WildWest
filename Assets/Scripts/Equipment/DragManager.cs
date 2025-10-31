using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;


public class DragManager : MonoBehaviour
{   
    public static DragManager instance {  get; private set; }

    private RectTransform dragItem;
    private SlotPosition dragItemSlot;
    private SlotPosition lastSlotPostion;

    private float lastSelectionTime = 0f;
    private float doubleClickThreshold = 0.5f;

    [SerializeField] Canvas canvas;

    private void Awake()
    {
        if (instance == null)
        {
            SetUp();
        }
        else
            Destroy(gameObject);
    }
    private void SetUp()
    {
        instance = this;
    }

    private void SelectItem()
    {

    }
    public bool ItemSelected(DragItem dragDrop, DropSlot dropSlot, PointerEventData eventData)
    {
        if (dragItem == null)
        {
            dragItemSlot = dragDrop.GetSlotPostion();
            if (lastSlotPostion.Compare(dragItemSlot) && Time.time - lastSelectionTime < doubleClickThreshold)
            {
                NewEquipmentManager.instance.CombineAllItems(dragItemSlot);
                return false;
            }
            dragItem = dragDrop.GetComponent<RectTransform>();
            lastSlotPostion = dragItemSlot;
            lastSelectionTime = Time.time;
            SelectionMode mode = GetSelectionModeForSelectItem(eventData, out int n);
            ItemStats itemStats = NewEquipmentManager.instance.SelectItem(dragItemSlot, mode, n);
            UIManager.instance.UpdateDragItem(dragDrop.transform, itemStats);
            return true;
        }
        else
        {
            ItemStats item = SlotSelected(dropSlot, eventData);
            if (item != null)
            {
                SlotPosition slotPosition = dropSlot.GetSlotPosition();
                lastSlotPostion = slotPosition;
                dragItem = dragDrop.GetComponent<RectTransform>();
                ItemStats itemStats = NewEquipmentManager.instance.LocalSelectItem(slotPosition, item);
                UIManager.instance.UpdateDragItem(dragDrop.transform, item);
                return true;
            }
            else
                return false;
        }
    }
    public ItemStats SlotSelected(DropSlot slot, PointerEventData eventData)
    {
        SlotPosition slotPosition = slot.GetSlotPosition();
        if (dragItem != null)
        {
            SelectionMode mode = GetSelectionModeForSelectSlot(eventData,out int n);
            return NewEquipmentManager.instance.MoveItemData(slotPosition, mode, n); 
        }
        Debug.Log("NIE UDALO SIE!!");
        return null;
    }
    public void UpdateSelected(ItemStats stats)
    {
        if (stats != null && stats.quantity >= 1)
            UIManager.instance.UpdateDragItem(dragItem, stats);
        else if(dragItem != null)
        {
            Destroy(dragItem.gameObject);
            dragItem = null;
        }
    }
    private SelectionMode GetSelectionModeForSelectItem(PointerEventData pointerEventData,out int n)
    {
        SelectionMode mode = SelectionMode.N;
        n = 1;

        if (pointerEventData.button == PointerEventData.InputButton.Left)
            mode = SelectionMode.All;
        else if (pointerEventData.button == PointerEventData.InputButton.Right)
            mode = SelectionMode.Half;
        return mode;
    }
    private SelectionMode GetSelectionModeForSelectSlot(PointerEventData pointerEventData, out int n)
    {
        SelectionMode mode = SelectionMode.N;
        n = 1;
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            if (InputManager.i.moveTheItem.inProgress)
                n = 5;
            else if (InputManager.i.moveAllTheItems.inProgress)
                n = 10;
            else
                mode = SelectionMode.All;
        }
        return mode;
    }
    private void Update()
    {
        if (dragItem != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 anchoredPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                mousePos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out anchoredPos
            );
            dragItem.anchoredPosition = anchoredPos;
        }
    }
}

