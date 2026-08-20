using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class UIThermometer : MonoBehaviour
{
    public float pointZero;
    public float pointMax;

    private RectTransform bar;
    private Image image;
    private float range;
    private Color belowZero,aboveZero;
    private bool isLow;

    private float minTemperature; 
    private float maxTemperature; 

    public void Awake()
    {
        range = pointMax - pointZero;
        bar = transform.GetComponent<RectTransform>();
        image = transform.GetComponent<Image>();
    }
    public void SetUpBar(UIThermometerConfig config)
    {
        this.belowZero = config.BelowZeroThermometerColor;
        this.aboveZero = config.AboveZeroThermometerColor;
        this.isLow = false; 
        this.image.color = aboveZero;
        this.minTemperature = config.minTemperature;
        this.maxTemperature = config.maxTemperature;
    }
    public void SetValue(float temperature)
    {
        float posY = Mathf.InverseLerp(minTemperature,maxTemperature,temperature) * range;
        if (temperature >= 0.5f && isLow)
        {
            image.color = aboveZero;
            isLow = false;
        }
        else if (temperature < 0.5f && !isLow)
        {
            image.color = belowZero;
            isLow = true;
        }
        this.bar.sizeDelta = new Vector2(bar.sizeDelta.x, posY + pointZero);
    }
}