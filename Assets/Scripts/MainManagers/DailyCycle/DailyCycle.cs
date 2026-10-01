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
        timeOfDayBar.SetUpBar(rangePrefab,WorldConfig.TimeConfig.rangesUI);
        seasonBar.SetUpBar(rangePrefab,WorldConfig.TimeConfig.Seasons);
        seasonBar.UpdateBar();
        thermometer.SetUpBar(WorldConfig.UIConfig.ThermometerConfig);

        lerpLight = new LerpLight(globalLight);
        lerpWeather = new LerpWeather(rainParticleSystem,snowParticleSystem,thermometer);
        lerpGlobalVolume = new LerpGlobalVolume(volume);

        timeOfDayBar.GetComponent<DynamicTooltipTrigger>().SetUp(() =>
        {
            var timeOfDay =  WorldConfig.TimeConfig.GetTimeOfDayUI(time.TimeOfDay);
            return new TooltipInfo
            (
                currentTimeString.GetLocalizedString() + " " +time.HourInt + ":" + time.MinuteInt.ToString("00") + "\n" +
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

        GoInGameCilientSystem.OnStartTimer += UpdateTime;
        GoInGameCilientSystem.OnStartTimer += UpdateDayCounter;
        GoInGameCilientSystem.OnStartTimer += UpdateTimeOfDay;
        GoInGameCilientSystem.OnStartTimer += SetSunColor;
        GoInGameCilientSystem.OnStartTimer += SetGlobalVolume;
        GoInGameCilientSystem.OnStartTimer += SetUpWeather;

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

        GoInGameCilientSystem.OnStartTimer -= UpdateTime;
        GoInGameCilientSystem.OnStartTimer -= UpdateDayCounter;
        GoInGameCilientSystem.OnStartTimer -= UpdateTimeOfDay;
        GoInGameCilientSystem.OnStartTimer -= SetSunColor;
        GoInGameCilientSystem.OnStartTimer -= SetGlobalVolume;
        GoInGameCilientSystem.OnStartTimer -= SetUpWeather;

        WeatherClientSystem.OnWeatherUpdate -= UpdateWeather;
        WeatherSetUpClientSystem.OnWeatherSetUp -= UpdateWeather;

        WeatherEventsSystem.OnLightning -= Lightning;
    }
    #endregion
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
    private void UpdateWeather(LocalWeather localWeather, CurrentTime currentTime)
    {
        if(weather == null)
            lerpWeather.Set(localWeather);
        else
            lerpWeather.Start(weather.Value,localWeather,currentTime.WorldTime,WorldConfig.WeatherConfig.WeatherUpdateLerpDuration);
        
        weather = localWeather;
    } 
    private void SetUpWeather(CurrentTime time)
    {
        Sounds.CreateAmbient(WorldConfig.SoundsConfig.WindAmbient);
        Sounds.CreateAmbient(WorldConfig.SoundsConfig.RainAmbient);
    }
    private void Lightning(float2 position)
    {
        Instantiate(lightningPrefab,new Vector3(position.x,position.y,position.y),Quaternion.identity);
        Sounds.CreateWorldSound(WorldConfig.SoundsConfig.Thunder,position);
        lerpLight.SetLightningColor(WorldConfig.WeatherConfig.stormConfig.LightningGlobalLightColor,time.WorldTime,WorldConfig.WeatherConfig.stormConfig.LightningColorLerpDuration);
    }
    #endregion
}   