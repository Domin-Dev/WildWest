using UnityEngine;
using UnityEngine.EventSystems;

public class SoundOnEvent : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
         Sounds.instance.Click();
    }
}
