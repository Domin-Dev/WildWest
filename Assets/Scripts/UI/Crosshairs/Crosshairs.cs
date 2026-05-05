using NUnit.Framework;
using UnityEngine;

public class Crosshairs : MonoBehaviour
{
    private static Crosshairs i;
    private RectTransform rectTransform;


    public static void Swtich(bool turnOn)
    {
        i.gameObject.SetActive(turnOn);
        Cursor.visible = !turnOn;
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
