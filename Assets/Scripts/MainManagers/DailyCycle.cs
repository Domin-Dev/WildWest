using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Scenes;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;



// public class Thermometer
// {
//     public RectTransform bar;
//     public Image image;
//     public float pointZero;
//     public float range;
//     Color low,high;
//     bool isLow;
//     public Thermometer(RectTransform rect, float pointMax, float pointZero, Color low,Color high)
//     {
//         this.bar = rect;
//         this.pointZero = pointZero;
//         range = pointMax - pointZero;
//         this.low = low;
//         this.high = high;
//         image = bar.GetComponent<Image>();
//         isLow = false; 
//         image.color = high;
//     }
//     public void SetValue(float value)
//     {
//         if (bar == null) return;
//         float posY = range * value;
//         if (value >= 0.5f  && isLow)
//         {
//           //  image.color = high;
//             isLow = false;
//         }
//         else if (value < 0.5f && !isLow)
//         {
//           //  image.color = low;
//             isLow = true;
//         }
//         this.bar.sizeDelta = new Vector2(bar.sizeDelta.x, posY + pointZero);
//     }
// }
public class DailyCycleUI : MonoBehaviour
{
    [SerializeField] private UIBar timeOfDayBar;
    [SerializeField] private UIBar seasonBar;
    [SerializeField] private TextMeshProUGUI dayCounter;
    [SerializeField] private GameObject rangePrefab;
    [SerializeField] private Light2D globalLight;
    
    public static DailyCycleUI instance { private set; get; }

    private Color startColor;
    private Color targetColor;
    private float startHour;
    private float lerpTime;
    private bool lerpColor;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
    public void Start()
    {
        timeOfDayBar.SetUpBar(rangePrefab,WorldConfig.TimeConfig.rangesUI);
        seasonBar.SetUpBar(rangePrefab,WorldConfig.TimeConfig.Seasons);
        seasonBar.UpdateBar();
    }
    public void OnEnable()
    {
        ClientTimeSystem.OnTimeUpdate += UpdateTime;
        ClientTimeSystem.OnNextDay += UpdateDayCounter;
        ClientTimeSystem.OnNextTimeOfDay += UpdateTimeOfDay;

        GoInGameCilientSystem.OnStartTimer += UpdateTime;
        GoInGameCilientSystem.OnStartTimer += UpdateDayCounter;
        GoInGameCilientSystem.OnStartTimer += UpdateTimeOfDay;
        GoInGameCilientSystem.OnStartTimer += SetSunColor;
    }
    public void OnDisable()
    {
        ClientTimeSystem.OnTimeUpdate -= UpdateTime;
        ClientTimeSystem.OnNextDay -= UpdateDayCounter;
        ClientTimeSystem.OnNextTimeOfDay -= UpdateTimeOfDay;

        GoInGameCilientSystem.OnStartTimer -= UpdateTime;
        GoInGameCilientSystem.OnStartTimer -= UpdateDayCounter;
        GoInGameCilientSystem.OnStartTimer -= UpdateTimeOfDay;
        GoInGameCilientSystem.OnStartTimer -= SetSunColor;
    }
    private void UpdateTime(CurrentTime time)
    {
        timeOfDayBar.SetValue(time.Hour/24f);
        UpdateGlobalLight(time.Hour);
    }
    private void UpdateDayCounter(CurrentTime time)
    {
        dayCounter.text = "Day " + time.Day;
        timeOfDayBar.UpdateBar(WorldConfig.TimeConfig.GetDailySchedule(time.season,time.Day).GetTimes());
        float season = ((time.Day - 1) % WorldConfig.TimeConfig.SeasonDuration) / (float)(WorldConfig.TimeConfig.SeasonDuration - 1);
        seasonBar.SetValue(((int)time.season + season) * 0.25f);
    }
    private void UpdateTimeOfDay(CurrentTime time)
    {
        (Color color, float lerpTime) = WorldConfig.TimeConfig.GetTimeOfDayColor(time);
        var schedule = WorldConfig.TimeConfig.GetDailySchedule(time.season,time.Day);
        float start = schedule.GetStartTimeOfDay(time.TimeOfDay);
        StartLerpColor(globalLight.color,color,start,lerpTime);
    }
    private void SetSunColor(CurrentTime time)
    {
        (Color color, float lerpT) = WorldConfig.TimeConfig.GetTimeOfDayColor(time);
        var schedule = WorldConfig.TimeConfig.GetDailySchedule(time.season,time.Day);
        float start = schedule.GetStartTimeOfDay(time.TimeOfDay);
        
        if(time.Hour - start >= lerpT || (time.Hour < start && 24 + time.Hour - start >= lerpT))
            globalLight.color = color;
        else
        {
            (Color preColor, float _) = WorldConfig.TimeConfig.GetPreviousTimeOfDayColor(time);        
            StartLerpColor(preColor,color,start,lerpT);
            UpdateGlobalLight(time.Hour);
        }
    }
    private void StartLerpColor(Color startColor,Color targetColor,float startHour,float lerptime)
    {
        Debug.Log("start!!");
        this.startColor = startColor;
        this.targetColor = targetColor;
        this.startHour = startHour;
        this.lerpTime = lerptime;
        lerpColor = true;
    }
    private void UpdateGlobalLight(float currentHour)
    { 
        if(lerpColor)
        {
            float duration = currentHour < startHour ? 24f + currentHour - startHour : currentHour - startHour;
            float progress = duration/lerpTime;
            Debug.Log("progress "+ progress);
            globalLight.color = Color.Lerp(startColor,targetColor,progress);
            if(progress >= 1f)
                lerpColor = false;
        }
    }
}
//     [SerializeField] List<DayTime> seasons = new List<DayTime>();
//     [SerializeField] private RectTransform timeOfDayTransform;
//     [SerializeField] private RectTransform timeOfDayPointer;
//     [SerializeField] private RectTransform timeOfSesonsDayPointer;
//     [SerializeField] private RectTransform thermometerTransform;


//     [SerializeField] private TextMeshProUGUI dayCounterText;

//     [SerializeField] private Color highTemperatureColor;
//     [SerializeField] private Color lowTemperatureColor;

//     public const int seasonDuration = 2;
//     public const int minutesPerDay = 1;
//     public readonly int ticksPerDay = TimeTickSystem.TicksPerMinute * minutesPerDay;
//     public readonly int ticksPerGameHour = (int)(TimeTickSystem.TicksPerMinute * (minutesPerDay / 24f));
    
//     int dayTimeInTicks = 0;
//     int dayCounter = 1;
//     int currentSeson = 0;


//     int [] seasonTimeArray = new int[3];
//     DayTime currentDayTime;
//     Color targetColor;
//     bool isColorChanging = false;

//     MyBar timeOfDayBar;
//     MyBar timeOfSesonsBar;
//     Thermometer thermometer;



//     public void Start()
//     {
//         SetUp();
//        //LoadSeason(0);
//         UpdateDayCounter();
//         TimeTickSystem.OnTick += IncreaseTime; 
//     }
//     private void IncreaseTime(object sender, TimeTickSystem.OnTickArgs e)
//     {
//         dayTimeInTicks += 10;
//         if (dayTimeInTicks > ticksPerDay)
//         {
//             dayTimeInTicks = 0;
//             dayCounter++;
//             UpdateDayCounter();
//             float value = GetSeasonValue();

//             timeOfSesonsBar.SetValue(value);
//             int index = (int)(value / 0.25f);
//             if (index < 4 && index != currentSeson)
//             {
//                 //LoadSeason(index);
//             }
//         }
//         timeOfDayBar.SetValue(dayTimeInTicks / (float)ticksPerDay);
//         thermometer.SetValue(dayTimeInTicks / (float)ticksPerDay);

//      //   CheckColorChanging();
//     }
//     //private void CheckColorChanging()
//     //{
//     //    if (isColorChanging)
//     //    {
//     //        light.color = Color.Lerp(light.color, targetColor, 0.05f);
//     //        if (light.color == targetColor) isColorChanging = false;
//     //    }
//     //    else
//     //    {
//     //        for (int i = 0; i < 3; i++)
//     //        { 
//     //            int result = seasonTimeArray[i] - dayTimeInTicks;
//     //            if (result < 0 &&  Math.Abs(result) < ticksPerGameHour)
//     //            {
//     //                isColorChanging = true;
//     //                targetColor = currentDayTime.GetColors()[i];
//     //                break;
//     //            }
//     //        }
//     //    }
//     //}

//     private void SetUp()
//     {
//         dayCounter = 1;

//         float max = timeOfDayTransform.sizeDelta.x - 2;
//         timeOfDayBar = new MyBar(timeOfDayPointer, max);

//         max = 160;
//         timeOfSesonsBar = new MyBar(timeOfSesonsDayPointer, max);
//         timeOfSesonsBar.SetValue(GetSeasonValue());

//         thermometer = new Thermometer(thermometerTransform,67, 19,lowTemperatureColor,highTemperatureColor);

//     }
//     private float GetSeasonValue()
//     {
//         int mod = dayCounter % (4 * seasonDuration);
//         if (mod == 0) return 1f;
//         return mod /(float)(4 * seasonDuration);
//     }
//     private void LoadSeason(int seasonIndex)
//     {
//         if (timeOfDayTransform == null) return;
//         currentSeson = seasonIndex;
//         currentDayTime = seasons[seasonIndex];
//         float hourWidth = timeOfDayTransform.sizeDelta.x / 24;
//         float[] times = currentDayTime.GetTimes();
       
//         SetTimeBar(0, hourWidth * times[0]);
//         seasonTimeArray[0] = 0;
//         int last = (int)(ticksPerGameHour * times[0]);
//         for (int i = 1; i < 3; i++)
//         {
//             SetTimeBar(i, hourWidth * times[i]);
//             seasonTimeArray[i] = last;
//             last = last + (int)(ticksPerGameHour * times[i]);
//         }
//     }
//     private void SetTimeBar(int index, float value)
//     {
//         RectTransform dayT = timeOfDayTransform.GetChild(index).GetComponent<RectTransform>();
//         dayT.sizeDelta = new Vector2(value, dayT.sizeDelta.y);
//     }
// }

