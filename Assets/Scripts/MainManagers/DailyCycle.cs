using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;



public class Thermometer
{
    public RectTransform bar;
    public Image image;
    public float pointZero;
    public float range;
    Color low,high;
    bool isLow;
    WhiteBalance whiteBalance;
    public Thermometer(WhiteBalance volume,RectTransform rect, float pointMax, float pointZero, Color low,Color high)
    {
        this.whiteBalance = volume;
        this.bar = rect;
        this.pointZero = pointZero;
        range = pointMax - pointZero;
        this.low = low;
        this.high = high;
        image = bar.GetComponent<Image>();
        isLow = false; 
        image.color = high;
    }


    public void SetValue(int value)
    {
        float posY = range * (value / DailyCycle.maxTemperature);
        //whiteBalance.temperature = value * ()

        if (value >= 0.5f  && isLow)
        {
            image.color = high;
            isLow = false;
        }
        else if (value < 0.5f && !isLow)
        {
            image.color = low;
            isLow = true;
        }

        this.bar.sizeDelta = new Vector2(bar.sizeDelta.x, posY + pointZero);
    }
}

[System.Serializable]
public class MyBar
{
    public RectTransform pointer;
    public float pointZero;
    public float range;

    public MyBar(RectTransform pointer, float pointMax)
    {
        this.pointer = pointer;
        this.pointZero = pointer.anchoredPosition.x;
        this.range = pointMax;
    }
    public void SetValue(float value)
    {
        float posX = range * value;
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
    [SerializeField] private RectTransform thermometerTransform;

    [SerializeField] private Light2D light;
    [SerializeField] private WhiteBalance whiteBalance;

    [SerializeField] private TextMeshProUGUI dayCounterText;

    [SerializeField] private Color highTemperatureColor;
    [SerializeField] private Color lowTemperatureColor;

    public const int seasonDuration = 2;
    public const int minutesPerDay = 1;

    public const int maxTemperature = 40;

    public readonly int ticksPerDay = TimeTickSystem.TicksPerMinute * minutesPerDay;
    public readonly int ticksPerGameHour = (int)(TimeTickSystem.TicksPerMinute * (minutesPerDay / 24f));
    
    int dayTimeInTicks = 0;
    int dayCounter = 1;
    int currentSeson = 0;


    int [] seasonTimeArray = new int[3];
    DayTime currentDayTime;
    Color targetColor;
    bool isColorChanging = false;

    MyBar timeOfDayBar;
    MyBar timeOfSesonsBar;
    Thermometer thermometer;



    public void Start()
    {
        SetUp();
        LoadSeason(0);
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
            float value = GetSeasonValue();

            timeOfSesonsBar.SetValue(value);
            int index = (int)(value / 0.25f);
            if (index < 4 && index != currentSeson)
            {
                LoadSeason(index);
            }
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

        thermometer = new Thermometer(whiteBalance, thermometerTransform,67, 19,lowTemperatureColor,highTemperatureColor);

    }
    private float GetSeasonValue()
    {
        int mod = dayCounter % (4 * seasonDuration);
        if (mod == 0) return 1f;
        return mod /(float)(4 * seasonDuration);
    }
    private void LoadSeason(int seasonIndex)
    {
        currentSeson = seasonIndex;
        currentDayTime = seasons[seasonIndex];
        float hourWidth = timeOfDayTransform.sizeDelta.x / 24;
        float[] times = currentDayTime.GetTimes();
       
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

