using UnityEngine;
using UnityEngine.EventSystems;

public class MouseIsOverEquipmentUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        UIManager.instance.mouseIsOverEQUI = true;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        UIManager.instance.mouseIsOverEQUI = false;
    }
}
