


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOverUIIgnor
{
    
}
public static class MouseOverUI
{
    public static bool MouseIsOverUI()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;    
        var raycastResult = new List<RaycastResult>();
        int counter = 0;
        foreach(var result in raycastResult)
        {
            if(result.gameObject.TryGetComponent<MouseOverUIIgnor>(out var component))
            {
                counter++;
            }
        }
        return raycastResult.Count > counter;
    }
}