using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[System.Serializable]
public class MyBar
{
    public RectTransform pointer;
    public float pointZero;
    public float pointMax;

    public MyBar(RectTransform pointer, float pointMax)
    {
        this.pointer = pointer;
        this.pointZero = pointer.anchoredPosition.x;
        this.pointMax = pointZero + pointMax;
    }
    public void SetValue(float value)
    {
        float posX = (pointMax - pointZero) * value;
        this.pointer.anchoredPosition = new Vector2(posX + pointZero, this.pointer.anchoredPosition.y);
    }
}


[System.Serializable]
public class DayTime
{
    [Range(0, 24)]
    public float dayTime;
    public Color dayLightColor;
    [Range(0, 24)]
    public float eveningTime;
    public Color eveningLightColor;
    [Range(0, 24)]
    public float nightTime;
    public Color nightLightColor;
    public Color[] GetColors()
    {
        Color[] colors = new Color[3];
        colors[0] = dayLightColor;
        colors[1] = eveningLightColor;
        colors[2] = nightLightColor;
        return colors;
    }
    public float[] GetTimes()
    {
        float[] times = new float[3];
        times[0] = dayTime;
        times[1] = eveningTime;
        times[2] = nightTime;
        return times;
    }
}
public class DailyCycle : MonoBehaviour
{
    [SerializeField] List<DayTime> seasons = new List<DayTime>();
    [SerializeField] private RectTransform timeOfDayTransform;
    [SerializeField] private RectTransform timeOfDayPointer;
    [SerializeField] private RectTransform timeOfSesonsDayPointer;

    [SerializeField] private Light2D light;

    [SerializeField] private TextMeshProUGUI dayCounterText;

    public const int seasonDuration = 2;
    public const int minutesPerDay = 1;
    public readonly int ticksPerDay = TimeTickSystem.TicksPerMinute * minutesPerDay;
    public readonly int ticksPerGameHour = (int)(TimeTickSystem.TicksPerMinute * (minutesPerDay / 24f));
    
    public int  dayTimeInTicks = 0;
    public int  dayCounter = 1;
   
    public int [] seasonTimeArray = new int[3];
    DayTime currentDayTime;
    public Color targetColor;
    bool isColorChanging = false;

    MyBar timeOfDayBar;
    MyBar timeOfSesonsBar;

    public void Start()
    {
        SetUp();
        LoadSeason(seasons[0]);
        UpdateDayCounter();
        TimeTickSystem.OnTick += IncreaseTime; 
    }

    
    private void IncreaseTime(object sender, TimeTickSystem.OnTickArgs e)
    {
        dayTimeInTicks += 10;

        if (dayTimeInTicks > ticksPerDay)
        {
            dayTimeInTicks = 0;
            dayCounter++;
            UpdateDayCounter();
            timeOfSesonsBar.SetValue(GetSeasonValue());
        }
        timeOfDayBar.SetValue(dayTimeInTicks / (float)ticksPerDay);
        CheckColorChanging();
    }
    private void CheckColorChanging()
    {
        if (isColorChanging)
        {
            light.color = Color.Lerp(light.color, targetColor, 0.05f);
            if (light.color == targetColor) isColorChanging = false;
        }
        else
        {
            for (int i = 0; i < 3; i++)
            { 
                int result = seasonTimeArray[i] - dayTimeInTicks;
                if (result < 0 &&  Math.Abs(result) < ticksPerGameHour)
                {
                    isColorChanging = true;
                    targetColor = currentDayTime.GetColors()[i];
                    break;
                }
            }
        }
    }
    private void UpdateDayCounter()
    {
        dayCounterText.text = "Day: " + dayCounter;
    }
    private void SetUp()
    {
        dayCounter = 1;

        float max = timeOfDayTransform.sizeDelta.x - 2;
        timeOfDayBar = new MyBar(timeOfDayPointer, max);

        max = 160;
        timeOfSesonsBar = new MyBar(timeOfSesonsDayPointer, max);
        timeOfSesonsBar.SetValue(GetSeasonValue());
    }

    private float GetSeasonValue()
    {
        int mod = dayCounter % (4 * seasonDuration);
        if (mod == 0) return 1f;
        return mod /(float)(4 * seasonDuration);
    }
    private void LoadSeason(DayTime season)
    {
        currentDayTime = season;
        float hourWidth = timeOfDayTransform.sizeDelta.x / 24;
        float[] times = season.GetTimes();
       
        SetTimeBar(0, hourWidth * times[0]);
        seasonTimeArray[0] = 0;
        int last = (int)(ticksPerGameHour * times[0]);
        for (int i = 1; i < 3; i++)
        {
            SetTimeBar(i, hourWidth * times[i]);
            seasonTimeArray[i] = last;
            last = last + (int)(ticksPerGameHour * times[i]);
        }
    }
    private void SetTimeBar(int index, float value)
    {
        RectTransform dayT = timeOfDayTransform.GetChild(index).GetComponent<RectTransform>();
        dayT.sizeDelta = new Vector2(value, dayT.sizeDelta.y);
    }
}

