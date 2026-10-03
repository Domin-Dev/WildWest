using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;


public class UIBar : MonoBehaviour
{
    [SerializeField] public RectTransform pointer;
    private RectTransform bar;
    private float pointZero;
    private float range;

    public void Awake()
    {
        bar = GetComponent<RectTransform>();
        range = bar.sizeDelta.x - 2;
        pointZero = pointer.anchoredPosition.x;
        SetValue(0f);
    }
    public void SetValue(float value)
    {
        if (this.pointer == null) return;
        float posX = range * value;
        this.pointer.anchoredPosition = new Vector2(posX + pointZero, this.pointer.anchoredPosition.y);
    }
    public void SetUpBar(GameObject rangeBarPrefab,RangeUI[] ranges)
    {
        foreach(var range in ranges)
        {
            var r = Instantiate(rangeBarPrefab,transform).transform;
            r.GetComponent<Image>().sprite = range.barSprite;
            r.GetChild(0).GetComponent<Image>().sprite = range.iconSprite;
        }
    }
    public void UpdateBar()
    {
        int count = transform.childCount;
        float[] values = new float[count];
        for(int i = 0; i < count; i++)
            values[i] = 1;
        UpdateBar(values);
    }

    public void UpdateBar(float[] values)
    {
        int count = transform.childCount;
        if(values.Length != count)
            return;
        float sum = values.Sum();
        float width = bar.sizeDelta.x;
        for(int i = 0; i < count; i++)
        {
            var rect = transform.GetChild(i).GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2((values[i] / sum * width),rect.sizeDelta.y);
        }
    }
}
