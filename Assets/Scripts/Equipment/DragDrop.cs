
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    private RectTransform rectTransform;
    public Transform parent;
    private CanvasGroup canvasGroup;

    public bool isInSlot;

    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetCanvas(Canvas canvas)
    {
        this.canvas = canvas;
    }

    public void IsInSlot()
    {
        isInSlot = true;
        parent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Middle && EquipmentManager.instance.IsNotSelected())
        {
            canvasGroup.alpha = 0.7f;
            canvasGroup.blocksRaycasts = false;
            rectTransform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            transform.SetParent(UIManager.instance.itemParent);
            isInSlot = false;
            EquipmentManager.instance.input = eventData.button;
            if (parent != null)
            {
                if (eventData.button == PointerEventData.InputButton.Left)
                {
                    EquipmentManager.instance.SelectedSlotTakeAll(parent.GetComponent<DropSlot>().GetSlotPosition());
                }
                else
                {
                    EquipmentManager.instance.SelectedSlotTakeHalf(parent.GetComponent<DropSlot>().GetSlotPosition());
                }
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (EquipmentManager.instance.input == eventData.button)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void ResetItem()
    {
        rectTransform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        EquipmentManager.instance.UnselectedSlot();
        if (parent != null)
        {
            transform.SetParent(parent);
            transform.SetAsFirstSibling();
        }
        rectTransform.anchoredPosition = Vector2.zero;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (EquipmentManager.instance.input == eventData.button)
        {
            rectTransform.localScale = Vector3.one;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            if (!isInSlot)
            {
                if(!UIManager.instance.mouseIsOverEQUI)
                {
                    EquipmentManager.instance.ThrowItem();
                    Destroy(gameObject);
                }
                else
                {
                    EquipmentManager.instance.UnselectedSlot();
                    if (parent != null)
                    {
                        transform.SetParent(parent);
                        transform.SetAsFirstSibling();
                    }
                    rectTransform.anchoredPosition = Vector2.zero;
                }
            }
            else
            {
                EquipmentManager.instance.ClearSelectedSlot();
            }                            
        }
    }
}
