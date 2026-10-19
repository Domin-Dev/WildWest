using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DailyCycle : MonoBehaviour
{
    public static DailyCycle instance { private set; get; }

    #region UI
    [SerializeField] private UIBar timeOfDayBar;
    [SerializeField] private UIBar seasonBar;
    [SerializeField] private UIThermometer thermometer;
    [SerializeField] private DynamicTooltipTrigger thermometerTrigger;
    [SerializeField] private TextMeshProUGUI dayCounter;
    [SerializeField] private GameObject rangePrefab;
    #endregion
    #region WeatherObjects
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Volume volume;
    [SerializeField] private GameObject lightningPrefab;
    #endregion
    #region LocalizedStrings
    [Space]
    [SerializeField] private LocalizedString currentTimeString;
    [SerializeField] private LocalizedString currentSeasonString;
    [SerializeField] private LocalizedString currentTimeOfDayString;
    [SerializeField] private LocalizedString BeginsInDaysString;
    [SerializeField] private LocalizedString temperatureString;
    #endregion
    #region Precipitation
    [Header("Precipitation")]
    [SerializeField] private ParticleSystem rainParticleSystem;
    [SerializeField] private ParticleSystem snowParticleSystem;
    #endregion
    #region Lerp
    private LerpLight lerpLight;
    private LerpWeather lerpWeather;
    private LerpGlobalVolume lerpGlobalVolume;
    private CurrentTime time;
    private LocalWeather? weather = null;
    #endregion

    #region UnityFunctions
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
        timeOfDayBar.SetUpBar(rangePrefab,WorldTimeConfig.Instance.rangesUI);
        seasonBar.SetUpBar(rangePrefab,WorldTimeConfig.Instance.Seasons);
        seasonBar.UpdateBar();
        thermometer.SetUpBar(UIConfig.Instance.ThermometerConfig);

        lerpLight = new LerpLight(globalLight);
        lerpWeather = new LerpWeather(rainParticleSystem,snowParticleSystem,thermometer);
        lerpGlobalVolume = new LerpGlobalVolume(volume);

        timeOfDayBar.GetComponent<DynamicTooltipTrigger>().SetUp(() =>
        {
            var timeOfDay =  WorldTimeConfig.Instance.GetTimeOfDayUI(time.TimeOfDay);
            return new TooltipInfo
            (
                currentTimeString.GetLocalizedString() + " " +time.HourInt + ":" + time.MinuteInt.ToString("00") + "\n" +
                currentTimeOfDayString.GetLocalizedString() + " : " + StringsHelper.GetColorfulString(timeOfDay.LocalizedString.GetLocalizedString(),timeOfDay.Color)
            );        
        });
        seasonBar.GetComponent<DynamicTooltipTrigger>().SetUp(() =>
        {
            var nextSeason = WorldTimeConfig.GetNextSeason(time.Season);
            int daysUntil =  WorldTimeConfig.Instance.SeasonDuration - (time.Day - 1) % WorldTimeConfig.Instance.SeasonDuration;
            
            var currentSeasonConfig = WorldTimeConfig.Instance.GetSeason(time.Season);
            var nextSeasonConfig = WorldTimeConfig.Instance.GetSeason(nextSeason);

            return new TooltipInfo
            (
                currentSeasonString.GetLocalizedString() + " : " + StringsHelper.GetColorfulString(currentSeasonConfig.seasonName.GetLocalizedString(),currentSeasonConfig.seasonColor) + "\n" +
                BeginsInDaysString.GetLocalizedString(StringsHelper.GetColorfulString(nextSeasonConfig.seasonName.GetLocalizedString(),nextSeasonConfig.seasonColor),daysUntil)
            ); 
        });
        thermometerTrigger.SetUp(() =>
        {
            return new TooltipInfo
            (
                temperatureString.GetLocalizedString() + " " + lerpWeather.Temperature.ToString("F1") +  " °C"
            );
        });
        
        rainParticleSystem.Stop(); 
        snowParticleSystem.Stop(); 
    }
    public void OnEnable()
    {
        ClientTimeSystem.OnTimeUpdate += UpdateTime;
        ClientTimeSystem.OnNextDay += UpdateDayCounter;
        ClientTimeSystem.OnNextTimeOfDay += UpdateTimeOfDay;
        ClientTimeSystem.OnNextSeason += SeasonStart;   

        GoInGameCilientSystem.OnStartTimer += OnTimeSet;
        GoInGameCilientSystem.OnStartTimer += SetUpWeather;
        ClientOnTimeSetSystem.OnTimeSet += OnTimeSet;

        WeatherClientSystem.OnWeatherUpdate += UpdateWeather;
        WeatherSetUpClientSystem.OnWeatherSetUp += UpdateWeather;

        WeatherEventsSystem.OnLightning += Lightning;
    }
    public void OnDisable()
    {
        ClientTimeSystem.OnTimeUpdate -= UpdateTime;
        ClientTimeSystem.OnNextDay -= UpdateDayCounter;
        ClientTimeSystem.OnNextTimeOfDay -= UpdateTimeOfDay;
        ClientTimeSystem.OnNextSeason -= SeasonStart;

        GoInGameCilientSystem.OnStartTimer -= OnTimeSet;
        GoInGameCilientSystem.OnStartTimer -= SetUpWeather;
        ClientOnTimeSetSystem.OnTimeSet -= OnTimeSet;

        WeatherClientSystem.OnWeatherUpdate -= UpdateWeather;
        WeatherSetUpClientSystem.OnWeatherSetUp -= UpdateWeather;

        WeatherEventsSystem.OnLightning -= Lightning;
    }
    #endregion
    #region Time
    private void OnTimeSet(CurrentTime time)
    {
        weather = null;
        UpdateTime(time);
        UpdateDayCounter(time);
        UpdateTimeOfDay(time);
        SetSunColor(time);
        SetGlobalVolume(time);
    }
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
        timeOfDayBar.UpdateBar(WorldTimeConfig.Instance.GetDailySchedule(time.Season,time.Day).GetTimes());
        float season = ((time.Day - 1) % WorldTimeConfig.Instance.SeasonDuration) / (float)(WorldTimeConfig.Instance.SeasonDuration);
        seasonBar.SetValue(((int)time.Season + season) * 0.25f);
    }
    private void UpdateTimeOfDay(CurrentTime time)
    {
        (Color color, float lerpTime) = WorldTimeConfig.Instance.GetTimeOfDayColor(time);
        var schedule = WorldTimeConfig.Instance.GetDailySchedule(time.Season,time.Day);
        float startHour = schedule.GetStartTimeOfDay(time.TimeOfDay);

        lerpLight.Start(globalLight.color,color,time.GetWorldTime(startHour),lerpTime);
    }
    private void SetSunColor(CurrentTime time)
    {
        (Color color, float lerpT) = WorldTimeConfig.Instance.GetTimeOfDayColor(time);
        var schedule = WorldTimeConfig.Instance.GetDailySchedule(time.Season,time.Day);
        double start = time.GetWorldTime(schedule.GetStartTimeOfDay(time.TimeOfDay));
        
        if(time.WorldTime - start >= lerpT)
            lerpLight.Set(color);
        else
        {
            (Color preColor, float _) = WorldTimeConfig.Instance.GetPreviousTimeOfDayColor(time);        
            lerpLight.Start(preColor,color,start,lerpT);
            lerpLight.Update(time.WorldTime);
        }
    }
    private void SetGlobalVolume(CurrentTime time)
    {
        double worldTimeStart = WorldTimeConfig.Instance.GetWorldTimeStartCurrentSeason(time); 
        var current = WorldTimeConfig.Instance.GetSeason(time.Season);

        if(worldTimeStart - worldTimeStart > current.whiteBalance.LerpDuration)
        {
            lerpGlobalVolume.Set(current.whiteBalance);
        }
        else
        {
            var previous = WorldTimeConfig.Instance.GetSeason(WorldTimeConfig.GetPreviousSeason(time.Season));
            lerpGlobalVolume.Start(previous.whiteBalance,current.whiteBalance,worldTimeStart,current.whiteBalance.LerpDuration);
        }
    }
    private void SeasonStart(CurrentTime time)
    {
        var current = WorldTimeConfig.Instance.GetSeason(time.Season);
        var previous = WorldTimeConfig.Instance.GetSeason(WorldTimeConfig.GetPreviousSeason(time.Season));
        lerpGlobalVolume.Start(previous.whiteBalance,current.whiteBalance,time.WorldTime,current.whiteBalance.LerpDuration);
    }
    #endregion
    #region Weather
    private void UpdateWeather(LocalWeather localWeather, CurrentTime currentTime)
    {
        if(weather == null)
            lerpWeather.Set(localWeather);
        else
            lerpWeather.Start(weather.Value,localWeather,currentTime.WorldTime,WeatherConfig.Instance.WeatherUpdateLerpDuration);
        
        weather = localWeather;
    } 
    private void SetUpWeather(CurrentTime time)
    {
        Sounds.CreateAmbient(SoundsConfig.Instance.WindAmbient);
        Sounds.CreateAmbient(SoundsConfig.Instance.RainAmbient);
    }
    private void Lightning(float2 position)
    {
        Instantiate(lightningPrefab,new Vector3(position.x,position.y,position.y),Quaternion.identity);
        Sounds.CreateWorldSound(SoundsConfig.Instance.Thunder,position);
        lerpLight.SetLightningColor(WeatherConfig.Instance.stormConfig.LightningGlobalLightColor,time.WorldTime,WeatherConfig.Instance.stormConfig.LightningColorLerpDuration);
    }
    #endregion
}   