using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ColorArgs : EventArgs
{ 
    public Color color;
    public ColorArgs(Color color)
    {
        this.color = color; 
    }
}

public class ColorPicker : MonoBehaviour
{
    [SerializeField] Slider sliderH;
    [SerializeField] Image backgroundH;

    [SerializeField] Slider sliderS;
    [SerializeField] Image backgroundS;

    [SerializeField] Slider sliderV;
    [SerializeField] Image backgroundV;

    int valueH = 180;
    int valueS = 50;
    int valueV = 50;

    public event EventHandler<ColorArgs> newColor;

    void Start()
    {
        Texture2D textureH = new Texture2D(360, 1);
        for (int x = 0; x < 360; x++)
        {
            Color color = Color.HSVToRGB(x/360f,1,1);
            textureH.SetPixel(x, 0, color);
        }
        textureH.Apply(true,true);
        backgroundH.sprite = Sprite.Create(textureH,new Rect(0,0,360,1),new Vector2(0.5f,0.5f));

        sliderH.onValueChanged.AddListener(OnValueHChanged);
        sliderH.value = valueH;

        sliderV.onValueChanged.AddListener(OnValueVChanged);
        sliderV.value = valueV;

        sliderS.onValueChanged.AddListener(OnValueSChanged);
        sliderS.value = valueS;
    }

    
    private void UpdateBarV()
    {
        Texture2D texture = new Texture2D(100, 1);
        for (int x = 0; x < 100; x++)
        {
            Color color = Color.HSVToRGB(valueH / 360f, valueS / 100f, x / 100f);
            texture.SetPixel(x, 0, color);
        }
        texture.Apply(true, true);
        backgroundV.sprite = Sprite.Create(texture, new Rect(0, 0, 100, 1), new Vector2(0.5f, 0.5f));
    }
    private void UpdateBarS()
    {
        Texture2D texture = new Texture2D(100, 1);
        for (int x = 0; x < 100; x++)
        {
            Color color = Color.HSVToRGB(valueH / 360f, x / 100f, valueV / 100f);
            texture.SetPixel(x, 0, color);
        }
        texture.Apply(true, true);
        backgroundS.sprite = Sprite.Create(texture, new Rect(0, 0, 100, 1), new Vector2(0.5f, 0.5f));
    }

    private void UpdateBars()
    {
        UpdateBarV();
        UpdateBarS();
        Color color = Color.HSVToRGB(valueH / 360f, valueS / 100f, valueV / 100f);
        newColor?.Invoke(this,new ColorArgs(color));
    }

    private void OnValueHChanged(float value)
    {
        valueH = (int)value;
        UpdateBars();
    }  
    private void OnValueSChanged(float value)
    {
        valueS = (int)value;
        UpdateBars();
    }  
    private void OnValueVChanged(float value)
    {
        valueV = (int)value;
        UpdateBars();
    }


}
