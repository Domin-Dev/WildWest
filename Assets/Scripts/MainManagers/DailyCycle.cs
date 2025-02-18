using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class MyBar
{
    public RectTransform pointer;
    public float pointZero;
    public float pointMax;

    public MyBar(RectTransform pointer, float pointMax)
    {
        this.pointer = pointer;
        this.pointZero = pointer.position.x;
        this.pointMax = pointZero + pointMax;
    }

    public void SetValue(float value)
    {
       
        float posX = (pointMax - pointZero) * value;
        Debug.Log(this.pointer.anchoredPosition + "" + posX );
        this.pointer.anchoredPosition = new Vector2(posX + pointZero, this.pointer.anchoredPosition.y);
    }

}


[System.Serializable]
public class DayTime
{
    [Range(0, 24)]
    public float dayTime;
    [Range(0, 24)]
    public float eveningTime;
    [Range(0, 24)]
    public float nightTime;
}
public class DailyCycle : MonoBehaviour
{
    [SerializeField] List<DayTime> seasons = new List<DayTime>();
    [SerializeField] private RectTransform timeOfDayTransform;
    [SerializeField] private RectTransform timeOfDayPointer;

    
    MyBar timeOfDayBar;
    float daytime = 0;

    private void Awake()
    {
        SetUp();
    }
    public void Start()
    {
        LoadSeason(seasons[0]);
        TimeTickSystem.On10Tick += IncreaseTime; 
    }

    private void IncreaseTime(object sender, TimeTickSystem.OnTickArgs e)
    {
        daytime += 0.005f;
        timeOfDayBar.SetValue(daytime);
    }

    private void SetUp()
    {
        float max = timeOfDayTransform.sizeDelta.x;
        timeOfDayBar = new MyBar(timeOfDayPointer, max);
    }
    private void LoadSeason(DayTime day)
    {
        float hourWidth = timeOfDayTransform.sizeDelta.x / 24;

        SetTimeBar(0, hourWidth * day.dayTime);
        SetTimeBar(1, hourWidth * day.eveningTime);
        SetTimeBar(2, hourWidth * day.nightTime);
    }

    private void SetTimeBar(int index, float value)
    {
        RectTransform dayT = timeOfDayTransform.GetChild(index).GetComponent<RectTransform>();
        dayT.sizeDelta = new Vector2(value, dayT.sizeDelta.y);
    }
}

