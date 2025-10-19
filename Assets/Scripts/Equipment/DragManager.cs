using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
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
    public bool ItemSelected(DragItem dragDrop, DropSlot dropSlot, SelectionMode selectionMode,int n = 1)
    {
        if (dragItem == null)
        {
            dragItem = dragDrop.GetComponent<RectTransform>();
            dragItemSlot = dragDrop.GetSlotPostion();
            ItemStats itemStats = NewEquipmentManager.instance.SelectItem(dragItemSlot, selectionMode, n);
            UIManager.instance.UpdateDragItem(dragDrop.transform, itemStats);
            return true;
        }
        else if (!SlotSelected(dropSlot))
            return ItemSelected(dragDrop, dropSlot, selectionMode, n);
        return false;
    }
    public bool SlotSelected(DropSlot slot)
    {
        SlotPosition slotPosition = slot.GetSlotPosition();
        if (dragItem != null)
        {
            NewEquipmentManager.instance.MoveItemData(slotPosition);
            Destroy(dragItem.gameObject);
            dragItem = null;
            return true;
        }
        Debug.Log("NIE UDALO SIE!!");
        return false;
    }

    public void UpdateSelected(ItemStats stats)
    {
        if (stats.quantity >= 1)
            UIManager.instance.UpdateDragItem(dragItem, stats);
        else
        {
            Destroy(dragItem.gameObject);
            dragItem = null;
        }
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

