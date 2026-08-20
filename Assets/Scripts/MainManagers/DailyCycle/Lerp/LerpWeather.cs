using System;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine;

public class LerpWeather : LerpValue
{    
    private LocalWeather start;
    private LocalWeather target;
    private double startTime;
    private float duration;

    private static int _WindDirection = Shader.PropertyToID("_WindDirection");
    private static int _DistortWindDirection = Shader.PropertyToID("_DistortWindDirection");
    private static int _Cloudiness = Shader.PropertyToID("_Cloudiness");

    private ParticleSystem rain;
    private ParticleSystem snow;
    private PrecipitationConfig config;

    public LerpWeather(ParticleSystem rain,ParticleSystem snow)
    {
        this.rain = rain;
        this.snow = snow;
        this.config = WorldConfig.WeatherConfig.PrecipitationConfig;
    }
    public void Start(LocalWeather startValue,LocalWeather targetValue,double startHour,float duration)
    {
        this.start = startValue;
        this.target = targetValue;
        this.startTime = startHour;
        this.duration = duration;
        isActive = true;
    }
    protected override void Lerp(double currentTime)
    {
        double t = currentTime - startTime;
        float progress = (float)t/duration;

        float2 wind = Vector2.Lerp(start.Wind,target.Wind,progress);
        float WindSpeed = math.length(wind);
        float cloudiness = Mathf.Lerp(start.Cloudiness,target.Cloudiness,progress);
        float precipitation = Mathf.Lerp(start.Precipitation,target.Precipitation,progress);

        float rotation = math.clamp(wind.x,-1,1) * config.MaxParticleRotation;
        var main = rain.main;
        main.startRotation = -rotation * Mathf.Deg2Rad;
        rain.transform.rotation = Quaternion.Euler(0,0,rotation);
        main.simulationSpeed = Mathf.Lerp(config.ParticleSimulationSpeedMin,config.ParticleSimulationSpeedMax,WindSpeed);
        
        var emission = rain.emission;
        float countValue = Mathf.InverseLerp(config.PrecipitationThreshold,1f,precipitation);
        emission.rateOverTime = Mathf.Lerp(config.ParticleCountMin,config.ParticleCountMax,countValue);
        
        Shader.SetGlobalVector(_WindDirection,new Vector4(wind.x,wind.y,0,0));
        Shader.SetGlobalVector(_DistortWindDirection,0.8f * new Vector4(wind.x,wind.y,0,0));
        Shader.SetGlobalFloat(_Cloudiness,cloudiness);

        if(progress >= 1f)
            isActive = false;    
    }
} 
