using NUnit.Framework;
using UnityEngine;

public class Crosshairs : MonoBehaviour
{
    private static Crosshairs i;
    private RectTransform rectTransform;
    private bool isCrossHairs;
    private bool isHide = false;

    public static void Swtich(bool turnOn)
    {
        if(!i.isHide)
        {
            i.gameObject.SetActive(turnOn);
            Cursor.visible = !turnOn;
        }
        
        i.isCrossHairs = turnOn;
    }

    public static void Hide()
    {
        i.gameObject.SetActive(false);
        i.isHide = true;
        Cursor.visible = true;
    }

    public static void Show()
    {
        i.isHide = false;
        if(i.isCrossHairs)
        {
            i.gameObject.SetActive(true);
            Cursor.visible = false;
        }
    }




    public static void SetSpread(float spread)
    {
        //Debug.Log("uwaga!!!  " + spread + " " + i == null);
        if(i != null)
        {
            i.rectTransform.sizeDelta = new Vector2(spread,spread) * 40;
        }
    }
    public void Awake()
    {
        if(i == null)
        {
            i = this; 
            rectTransform = GetComponent<RectTransform>();
        } 
        else
            Destroy(gameObject);
    }
    public void Update()
    {
        Vector2 position = Input.mousePosition;
        transform.position = position;
    }
}
