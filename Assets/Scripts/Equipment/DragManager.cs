using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public class DragManager : MonoBehaviour
{ 
    public static DragManager instance {  get; private set; }

    private RectTransform dragItem;


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
    public void ItemSelected(DragDrop dragDrop)
    {
        dragItem = dragDrop.GetComponent<RectTransform>();
    }
    public bool SlotSelected(DropSlot slot)
    {
        SlotPosition slotPosition = slot.GetSlotPosition();

        if (dragItem != null && NewEquipmentManager.instance.SlotIsEmpty(slotPosition))
        {    
            DragDrop item = dragItem.GetComponent<DragDrop>();
            SlotPosition itemPostion = item.GetSlotPostion();
            NewEquipmentManager.instance.MoveItemData(itemPostion, slotPosition);
            item.SetSlot(slot.transform as RectTransform);
            dragItem = null;
            return true;
        }
        Debug.Log("NIE UDALO SIE!!");
        return false;
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

