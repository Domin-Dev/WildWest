using UnityEngine;
using UnityEngine.EventSystems;

public class SoundOnEvent : MonoBehaviour,IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("sound!!!");
        Sounds.instance.Click();
    }
}
