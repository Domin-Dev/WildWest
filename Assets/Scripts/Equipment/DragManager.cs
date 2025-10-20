using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;


public class DragManager : MonoBehaviour
{ 
    public static DragManager instance {  get; private set; }

    private RectTransform dragItem;
    private SlotPosition dragItemSlot;


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
    public bool ItemSelected(DragItem dragDrop, DropSlot dropSlot, PointerEventData eventData)
    {
        if (dragItem == null)
        {
            dragItem = dragDrop.GetComponent<RectTransform>();
            dragItemSlot = dragDrop.GetSlotPostion();
            SelectionMode mode = GetSelectionModeForSelectItem(eventData,out int n);
            ItemStats itemStats = NewEquipmentManager.instance.SelectItem(dragItemSlot, mode, n);
            UIManager.instance.UpdateDragItem(dragDrop.transform, itemStats);
            return true;
        }
        else if (!SlotSelected(dropSlot,eventData))
            return ItemSelected(dragDrop, dropSlot, eventData);
        return false;
    }
    public bool SlotSelected(DropSlot slot, PointerEventData eventData)
    {
        SlotPosition slotPosition = slot.GetSlotPosition();
        if (dragItem != null)
        {
            SelectionMode mode = GetSelectionModeForSelectSlot(eventData,out int n);
            NewEquipmentManager.instance.MoveItemData(slotPosition,mode,n);
            return true;
        }
        Debug.Log("NIE UDALO SIE!!");
        return false;
    }

    public void UpdateSelected(ItemStats stats)
    {
        Debug.Log("stats! " + stats.ToString());
        if (stats != null && stats.quantity >= 1)
            UIManager.instance.UpdateDragItem(dragItem, stats);
        else
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
            mode = SelectionMode.All;
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

