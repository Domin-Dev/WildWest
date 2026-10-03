using System;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine;



public class LerpWeather : LerpValue
{    
    private enum PrecipitationState
    {
        None,
        Rain,
        Snow,
        RainAndSnow
    }
    private LocalWeather start;
    private LocalWeather target;
    private double startTime;
    private float duration;

    private static int _WindDirection = Shader.PropertyToID("_WindDirection");
    private static int _DistortWindDirection = Shader.PropertyToID("_DistortWindDirection");
    private static int _Cloudiness = Shader.PropertyToID("_Cloudiness");
    private static int _FogIntensity = Shader.PropertyToID("_FogIntensity");

    private UIThermometer thermometer;
    private ParticleSystem rain;
    private ParticleSystem snow;
    private PrecipitationConfig precipitationConfig;
    private FogConfig fogConfig;
    private PrecipitationState state;


    public float Temperature { private set; get;}

    public LerpWeather(ParticleSystem rain,ParticleSystem snow,UIThermometer thermometer)
    {
        this.thermometer = thermometer;
        this.rain = rain;
        this.snow = snow;
        this.precipitationConfig = WorldConfig.WeatherConfig.PrecipitationConfig;
        this.fogConfig = WorldConfig.WeatherConfig.fogConfig;
        this.state = PrecipitationState.None;
    }
    public void Start(LocalWeather startValue,LocalWeather targetValue,double startHour,float duration)
    {
        this.start = startValue;
        this.target = targetValue;
        this.startTime = startHour;
        this.duration = duration;
        isActive = true;
    }
    public void Set(LocalWeather value)
    {
        Set(value.Wind,value.Precipitation,value.Cloudiness,value.Temperature,value.FogIntensity);
        Simulate(snow);
        Simulate(rain);
    }

    private void Simulate(ParticleSystem particleSystem)
    {
        if(particleSystem.isPlaying)
        {
            particleSystem.Simulate(4f,true,true);
            particleSystem.Play();
        }
    }

    protected override void Lerp(double currentTime)
    {
        double t = currentTime - startTime;
        float progress = (float)t/duration;

        float2 wind = Vector2.Lerp(start.Wind,target.Wind,progress);
        float cloudiness = Mathf.Lerp(start.Cloudiness,target.Cloudiness,progress);
        float precipitation = Mathf.Lerp(start.Precipitation,target.Precipitation,progress);
        float temperature = Mathf.Lerp(start.Temperature,target.Temperature,progress);
        float fog = Mathf.Lerp(start.FogIntensity,target.FogIntensity,progress);

        Set(wind,precipitation,cloudiness,temperature,fog);

        if(progress >= 1f)
            isActive = false;    
    }

    private void Set(float2 wind, float precipitation,float cloudiness,float temperature,float fogIntensity)
    {
        Temperature = temperature;
        float windSpeed = math.length(wind);
        bool isPrecipitation = precipitation > precipitationConfig.PrecipitationThreshold;
        if(fogIntensity < fogConfig.FogThreshold) fogIntensity = 0;

        if(isPrecipitation)
        {
            float particleCount = Mathf.InverseLerp(precipitationConfig.PrecipitationThreshold,1f,precipitation);
            bool IsRaining = temperature > precipitationConfig.rainTemperatureThreshold;
            bool IsSnowing = temperature < precipitationConfig.rainTemperatureThreshold;
            PrecipitationState nextState = PrecipitationState.None;

            if(IsRaining)
            {
                var main = rain.main;
                var emission = rain.emission;

                float rotation = math.clamp(wind.x,-1,1) * precipitationConfig.RainMaxRotation;
                main.startRotation = -rotation * Mathf.Deg2Rad;
                rain.transform.rotation = Quaternion.Euler(0,0,rotation);
                main.simulationSpeed = Mathf.Lerp(precipitationConfig.RainSimulationSpeedMin,precipitationConfig.RainSimulationSpeedMax,windSpeed);
                emission.rateOverTime = Mathf.Lerp(precipitationConfig.RainParticleCountMin,precipitationConfig.RainParticleCountMax,particleCount);
                nextState = PrecipitationState.Rain;
            }
            
            if(IsSnowing)
            {
                var main = snow.main;
                var emission = snow.emission;
                var velocity =  snow.velocityOverLifetime;

                float velocityX = math.clamp(wind.x,-1,1) * precipitationConfig.SnowMaxVelocityX;
                main.simulationSpeed = Mathf.Lerp(precipitationConfig.SnowSimulationSpeedMin,precipitationConfig.SnowSimulationSpeedMax,windSpeed);
                emission.rateOverTime = Mathf.Lerp(precipitationConfig.SnowParticleCountMin,precipitationConfig.SnowParticleCountMax,particleCount);
                velocity.x = new ParticleSystem.MinMaxCurve(velocityX * 0.75f,velocityX);
                nextState = PrecipitationState.Snow;
            }

            if(IsSnowing && IsRaining)
                nextState = PrecipitationState.RainAndSnow;

            UpdatePrecipitationState(nextState);
        }
        else
            UpdatePrecipitationState(PrecipitationState.None);
        
     
        Shader.SetGlobalVector(_WindDirection,new Vector4(wind.x,wind.y,0,0));
        Shader.SetGlobalVector(_DistortWindDirection,0.8f * new Vector4(wind.x,wind.y,0,0));
        Shader.SetGlobalFloat(_Cloudiness,cloudiness);
        Shader.SetGlobalFloat(_FogIntensity,fogIntensity);


        Sounds.UpdateAmbient("Wind",windSpeed);

        if(rain.isPlaying)
            Sounds.UpdateAmbient("Rain",precipitation);
        else
            Sounds.UpdateAmbient("Rain",0);

        thermometer.SetValue(temperature);
    }
    private void UpdatePrecipitationState(PrecipitationState newState)
    {
        if(newState == state) return;

        Debug.Log("zmian!" + newState + " old " + state);

        switch(newState)
        {
            case PrecipitationState.Rain:
                if(!rain.isPlaying)
                    rain.Play();
                break;
            case PrecipitationState.Snow:
                if(!snow.isPlaying)
                    snow.Play();
                break;
            case PrecipitationState.RainAndSnow:
                if(!snow.isPlaying)
                    snow.Play();

                if(!rain.isPlaying)
                    rain.Play();
                break;
        }

        switch(state)
        {
            case PrecipitationState.Rain:
                if(newState != PrecipitationState.RainAndSnow && rain.isPlaying)
                    rain.Stop();
                break;
            case PrecipitationState.Snow:
                if(newState != PrecipitationState.RainAndSnow && snow.isPlaying)
                    snow.Stop();
                break;
            case PrecipitationState.RainAndSnow:
                if(newState != PrecipitationState.Snow && snow.isPlaying)
                    snow.Stop();
                    
                if(newState != PrecipitationState.Rain && rain.isPlaying)
                    rain.Stop();
                break;
        }

        state = newState;
    }
} 
