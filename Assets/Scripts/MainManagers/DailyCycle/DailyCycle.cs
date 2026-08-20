using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DailyCycleUI : MonoBehaviour
{
    [SerializeField] private UIBar timeOfDayBar;
    [SerializeField] private UIBar seasonBar;
    [SerializeField] private UIThermometer thermometer;
    [SerializeField] private DynamicTooltipTrigger thermometerTrigger;

    [SerializeField] private TextMeshProUGUI dayCounter;
    [SerializeField] private GameObject rangePrefab;
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Volume volume;
    
    [Space]

    [SerializeField] private LocalizedString currentTimeString;
    [SerializeField] private LocalizedString currentSeasonString;
    [SerializeField] private LocalizedString currentTimeOfDayString;
    [SerializeField] private LocalizedString BeginsInDaysString;
    [SerializeField] private LocalizedString temperatureString;

    [Header("Rain")]

    [SerializeField] private ParticleSystem rainParticleSystem;
    [SerializeField] private ParticleSystem snowParticleSystem;
 
    public static DailyCycleUI instance { private set; get; }
    private LerpLight lerpLight;
    private LerpWeather lerpWeather;
    private LerpGlobalVolume lerpGlobalVolume;
    private CurrentTime time;
    private LocalWeather weather;




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
        thermometer.SetUpBar(WorldConfig.UIConfig.ThermometerConfig);

        lerpLight = new LerpLight(globalLight);
        lerpWeather = new LerpWeather(rainParticleSystem,snowParticleSystem);
        lerpGlobalVolume = new LerpGlobalVolume(volume);

        timeOfDayBar.GetComponent<DynamicTooltipTrigger>().SetUp(() =>
        {
            int hour = (int) time.Hour;
            int minute = (int)((time.Hour - hour) * 60f);
            var timeOfDay =  WorldConfig.TimeConfig.GetTimeOfDayUI(time.TimeOfDay);

            return new TooltipInfo
            (
                currentTimeString.GetLocalizedString() + " " +(int)hour + ":" + minute.ToString("00") + "\n" +
                currentTimeOfDayString.GetLocalizedString() + " : " + UIStringsHelper.GetColorfulString(timeOfDay.LocalizedString.GetLocalizedString(),timeOfDay.Color)
            );        
        });
        seasonBar.GetComponent<DynamicTooltipTrigger>().SetUp(() =>
        {
            var nextSeason = WorldTimeConfig.GetNextSeason(time.Season);
            int daysUntil =  WorldConfig.TimeConfig.SeasonDuration - (time.Day - 1) % WorldConfig.TimeConfig.SeasonDuration;
            
            var currentSeasonConfig = WorldConfig.TimeConfig.GetSeason(time.Season);
            var nextSeasonConfig = WorldConfig.TimeConfig.GetSeason(nextSeason);

            return new TooltipInfo
            (
                currentSeasonString.GetLocalizedString() + " : " + UIStringsHelper.GetColorfulString(currentSeasonConfig.seasonName.GetLocalizedString(),currentSeasonConfig.seasonColor) + "\n" +
                BeginsInDaysString.GetLocalizedString(UIStringsHelper.GetColorfulString(nextSeasonConfig.seasonName.GetLocalizedString(),nextSeasonConfig.seasonColor),daysUntil)
            ); 
        });
        thermometerTrigger.SetUp(() =>
        {
            return new TooltipInfo
            (
                temperatureString.GetLocalizedString() + " " + weather.Temperature.ToString("F1") +  " °C"
            );
        });
        
        rainParticleSystem.Stop(); 
    }
    public void OnEnable()
    {
        ClientTimeSystem.OnTimeUpdate += UpdateTime;
        ClientTimeSystem.OnNextDay += UpdateDayCounter;
        ClientTimeSystem.OnNextTimeOfDay += UpdateTimeOfDay;
        ClientTimeSystem.OnNextSeason += SeasonStart;   

        GoInGameCilientSystem.OnStartTimer += UpdateTime;
        GoInGameCilientSystem.OnStartTimer += UpdateDayCounter;
        GoInGameCilientSystem.OnStartTimer += UpdateTimeOfDay;
        GoInGameCilientSystem.OnStartTimer += SetSunColor;
        GoInGameCilientSystem.OnStartTimer += SetGlobalVolume;
        GoInGameCilientSystem.OnStartTimer += SetUpWeather;


        ClientWeatherSystem.OnWeatherUpdate += UpdateWeather;
    }
    public void OnDisable()
    {
        ClientTimeSystem.OnTimeUpdate -= UpdateTime;
        ClientTimeSystem.OnNextDay -= UpdateDayCounter;
        ClientTimeSystem.OnNextTimeOfDay -= UpdateTimeOfDay;
        ClientTimeSystem.OnNextSeason -= SeasonStart;

        GoInGameCilientSystem.OnStartTimer -= UpdateTime;
        GoInGameCilientSystem.OnStartTimer -= UpdateDayCounter;
        GoInGameCilientSystem.OnStartTimer -= UpdateTimeOfDay;
        GoInGameCilientSystem.OnStartTimer -= SetSunColor;
        GoInGameCilientSystem.OnStartTimer -= SetGlobalVolume;
        GoInGameCilientSystem.OnStartTimer -= SetUpWeather;

        ClientWeatherSystem.OnWeatherUpdate -= UpdateWeather;
    }

    #region Time
    private void UpdateTime(CurrentTime time)
    {
        this.time = time;
        timeOfDayBar.SetValue(time.Hour/24f);
        
        lerpLight.Update(time.WorldTime);
        lerpWeather.Update(time.WorldTime);
        lerpGlobalVolume.Update(time.WorldTime);
    }
    private void UpdateDayCounter(CurrentTime time)
    {
        dayCounter.text = "Day " + time.Day;
        timeOfDayBar.UpdateBar(WorldConfig.TimeConfig.GetDailySchedule(time.Season,time.Day).GetTimes());
        float season = ((time.Day - 1) % WorldConfig.TimeConfig.SeasonDuration) / (float)(WorldConfig.TimeConfig.SeasonDuration);
        seasonBar.SetValue(((int)time.Season + season) * 0.25f);
    }
    private void UpdateTimeOfDay(CurrentTime time)
    {
        (Color color, float lerpTime) = WorldConfig.TimeConfig.GetTimeOfDayColor(time);
        var schedule = WorldConfig.TimeConfig.GetDailySchedule(time.Season,time.Day);
        float startHour = schedule.GetStartTimeOfDay(time.TimeOfDay);

        lerpLight.Start(globalLight.color,color,time.GetWorldTime(startHour),lerpTime);
    }
    private void SetSunColor(CurrentTime time)
    {
        (Color color, float lerpT) = WorldConfig.TimeConfig.GetTimeOfDayColor(time);
        var schedule = WorldConfig.TimeConfig.GetDailySchedule(time.Season,time.Day);
        double start = time.GetWorldTime(schedule.GetStartTimeOfDay(time.TimeOfDay));
        
        if(time.WorldTime - start >= lerpT)
            lerpLight.Set(color);
        else
        {
            (Color preColor, float _) = WorldConfig.TimeConfig.GetPreviousTimeOfDayColor(time);        
            lerpLight.Start(preColor,color,start,lerpT);
            lerpLight.Update(time.WorldTime);
        }
    }
    private void SetGlobalVolume(CurrentTime time)
    {
        double worldTimeStart = WorldConfig.TimeConfig.GetWorldTimeStartCurrentSeason(time); 
        var current = WorldConfig.TimeConfig.GetSeason(time.Season);

        if(worldTimeStart - worldTimeStart > current.whiteBalance.LerpDuration)
        {
            lerpGlobalVolume.Set(current.whiteBalance);
        }
        else
        {
            var previous = WorldConfig.TimeConfig.GetSeason(WorldTimeConfig.GetPreviousSeason(time.Season));
            lerpGlobalVolume.Start(previous.whiteBalance,current.whiteBalance,worldTimeStart,current.whiteBalance.LerpDuration);
        }
    }
    private void SeasonStart(CurrentTime time)
    {
        var current = WorldConfig.TimeConfig.GetSeason(time.Season);
        var previous = WorldConfig.TimeConfig.GetSeason(WorldTimeConfig.GetPreviousSeason(time.Season));
        lerpGlobalVolume.Start(previous.whiteBalance,current.whiteBalance,time.WorldTime,current.whiteBalance.LerpDuration);
    }
    #endregion
   
    #region Weather
    private void UpdateWeather(LocalWeather previousLocalWeather,LocalWeather localWeather, CurrentTime currentTime)
    {
        lerpWeather.Start(previousLocalWeather,localWeather,currentTime.WorldTime,WorldConfig.WeatherConfig.WeatherUpdateLerpDuration);
        thermometer.SetValue(localWeather.Temperature);
        weather = localWeather;

        Sounds.UpdateAmbient("Wind",localWeather.WindSpeed);
        Sounds.UpdateAmbient("Rain",localWeather.Precipitation);
        
        Debug.Log(localWeather.IsRaining + " " + previousLocalWeather.IsRaining);

        if(localWeather.IsRaining)
            UpdatePrecipitation(localWeather);   
        else if(previousLocalWeather.IsRaining)
            StopPrecipitation(localWeather); 
    } 
    private void SetUpWeather(CurrentTime time)
    {
        Sounds.CreateAmbient(WorldConfig.SoundsConfig.WindAmbient);
        Sounds.CreateAmbient(WorldConfig.SoundsConfig.RainAmbient);
    }
    private void UpdatePrecipitation(LocalWeather localWeather)
    {
        if(!rainParticleSystem.isPlaying)
            rainParticleSystem.Play();
    }
    private void StopPrecipitation(LocalWeather localWeather)
    {

        if(rainParticleSystem.isPlaying)
            rainParticleSystem.Stop();
    }

    #endregion
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

