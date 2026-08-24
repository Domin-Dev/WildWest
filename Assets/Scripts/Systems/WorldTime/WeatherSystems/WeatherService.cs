using System;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public static class WeatherService
{ 

    private static WeatherConfig weatherConfig
    {
        get
        {
            if(_weatherConfig == null)
                _weatherConfig = WorldConfig.WeatherConfig;
            return _weatherConfig;
        }
    }
    private static WeatherConfig _weatherConfig;

    private static float Noise(float2 position, int seed, float scale)
    {
        float offsetX = (seed * 0.12345f) % 100000f;
        float offsetY = (seed * 0.54321f) % 100000f;
        return math.clamp(Mathf.PerlinNoise(position.x * scale + offsetX, position.y * scale + offsetY),0f,1f);
    }
    public static LocalWeather GetWeather(int worldSeed,in CurrentTime currentTime, float2 position)
    {
        float2 windDir = GetWindDirection(position,currentTime.WorldTime,worldSeed,out float windSpeed);
        Vector2 weatherPosition = position - windDir * (float)currentTime.WorldTime * 0.1f;
        var seasonConfig = WorldConfig.TimeConfig.GetSeason(currentTime.Season);

        float temperature = GetTemperature(currentTime.Hour,seasonConfig,weatherPosition,worldSeed);
        float cloudiness = GetCloudiness(currentTime.Hour,seasonConfig,weatherPosition,worldSeed);
        float precipitation = GetPrecipitation(currentTime.Hour,seasonConfig,weatherPosition,worldSeed,cloudiness);
        float fogIntensity = GetFogIntensity(currentTime.Hour,seasonConfig,weatherPosition,worldSeed,windSpeed,precipitation);
        float storm = GetStorm(currentTime.Hour,seasonConfig,weatherPosition,worldSeed,windSpeed,precipitation,cloudiness);

        return new LocalWeather()
        {
            FogIntensity = fogIntensity,
            Storm = storm,
            Wind = windDir,
            Cloudiness = cloudiness,
            Temperature = temperature,
            Precipitation = precipitation,
            IsRaining = precipitation >= WorldConfig.WeatherConfig.PrecipitationConfig.PrecipitationThreshold
        }; 
    }
    private static float2 GetWindDirection(float2 position,double worldTime,int worldSeed,out float windSpeed)
    {
        float2 windNoisePos = position * 0.1f + (float)worldTime * 0.3f;
        float2 windDir = ValueToDirection(Noise(windNoisePos, worldSeed + 1, 0.03f));
        windSpeed = Noise(windNoisePos, worldSeed + 2, 0.03f);
    
        if (math.lengthsq(windDir) < 0.0001f)
            windDir = new float2(1f, 0f);
            
        windDir = math.normalize(windDir) * windSpeed;
        return windDir; 
    } 
    private static float GetTemperature(float hour,SeasonConfig seasonConfig,float2 weatherPosition,int worldSeed)
    {
        float noise = Noise(weatherPosition, worldSeed + 3, 0.03f);    
        float temperature = seasonConfig.seasonWeather.BaseTemperature + (noise * 2f - 1) * seasonConfig.seasonWeather.DailyVariation;
        temperature += GetDailyTemperature(hour,seasonConfig.seasonWeather.DailyAmplitude);
        return temperature;
    }
    private static float GetPrecipitation(float hour,SeasonConfig seasonConfig,float2 weatherPosition,int worldSeed,float cloudiness)
    {
        float noise = Noise(weatherPosition, worldSeed + 4, 0.03f);
        float precipitation = Mathf.Clamp01(seasonConfig.seasonWeather.BasePrecipitation + noise + cloudiness * weatherConfig.PrecipitationConfig.CloudinessInfluenceOnPrecipitation);
        return precipitation;
    }
    private static float GetCloudiness(float hour,SeasonConfig seasonConfig,float2 weatherPosition,int worldSeed)
    {
        float noise = Noise(weatherPosition, worldSeed + 5, 0.03f);
        float cloudiness = Mathf.Clamp01(seasonConfig.seasonWeather.BaseCloudiness + noise);
        return cloudiness;
    }
    private static float GetFogIntensity(float hour,SeasonConfig seasonConfig, float2 weatherPosition,int worldSeed,float windSpeed,float precipitation)
    {
        float noise = Noise(weatherPosition, worldSeed + 6, 0.03f) + seasonConfig.seasonWeather.BaseFogIntensity + weatherConfig.fogConfig.fogByTime.Evaluate(hour);
        float fog = Mathf.Clamp01(noise + weatherConfig.fogConfig.WindSpeedInfluenceOnFog * windSpeed + weatherConfig.fogConfig.PrecipitationInfluenceOnFog * precipitation);
        return fog;
    }
    private static float GetStorm(float hour,SeasonConfig seasonConfig,float2 weatherPosition,int worldSeed,float windSpeed,float precipitation, float cloudiness)
    {
        float noise = Noise(weatherPosition, worldSeed + 7, 0.03f) * seasonConfig.seasonWeather.BaseStorm
        + windSpeed * weatherConfig.stormConfig.WindSpeedInfluenceOnStorm 
        + precipitation * weatherConfig.stormConfig.PrecipitationInfluenceOnStorm 
        + cloudiness * weatherConfig.stormConfig.CloudinessInfluenceOnStorm;
        float storm = Mathf.Clamp01(noise);
        return storm;
    }
    private static float GetDailyTemperature(float hour,float amplitude)
    {
        float angle = (hour - (9f)) / 24f * Mathf.PI * 2f;
        return Mathf.Sin(angle) * amplitude;
    }
    private static float2 ValueToDirection(float value)
    {
        float angle = value * math.PI * 2f;
        return new float2(
            math.cos(angle),
            math.sin(angle)
        );
    }
}