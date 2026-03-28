
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragItem : MonoBehaviour, IPointerDownHandler, IGetSlotPosition
{
    private RectTransform rectTransform;
    public Transform parent;
    private CanvasGroup canvasGroup;

    public bool isInSlot;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }
    public SlotPosition GetSlotPosition()
    {
        if(parent != null)
            return parent.GetComponent<DropSlot>().GetSlotPosition();
        else
            return default;
    }

    private void OnDestroy()
    {
        if(parent != null)
            parent.GetComponent<DropSlot>().slotClick -= ClickItem;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ClickItem(eventData);
    }

    private void ClickItem(PointerEventData eventData)
    {
        DropSlot dropSlot = parent.GetComponent<DropSlot>();
        if (DragManager.instance.ItemSelected(this, dropSlot,eventData))
        {
            dropSlot.slotClick -= ClickItem;
            SlotSelected();
            NewEquipmentManager.instance.LocalUpdateSlotIndex(dropSlot.GetSlotPosition());
        }
    }

    public void SlotSelected()
    {
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(UIManager.instance.itemParent);
        rectTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
    }
    public void SetSlot(RectTransform newSlot)
    {
        rectTransform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        transform.SetParent(newSlot);
        parent = newSlot;
        transform.SetAsFirstSibling();
        newSlot.GetComponent<DropSlot>().slotClick += ClickItem;

        rectTransform.anchoredPosition = Vector2.zero;
    }
    public void IsInSlot()
    {
        isInSlot = true;
        parent = transform.parent;
        parent.GetComponent<DropSlot>().slotClick += ClickItem;
    }


    public void ResetItem()
    {

    }

}
