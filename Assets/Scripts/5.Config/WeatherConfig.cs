using UnityEngine;
using EditorAttributes;
using Unity.Mathematics;
using NaughtyAttributes;


[CreateAssetMenu(fileName = "WeatherConfig", menuName = "GameAsset/ConfigFiles/WeatherConfig")]
public class WeatherConfig : Config<WeatherConfig>
{
    [Header("Weather Settings")]

    [Rename("Weather Update Period"),Suffix("Game Hour")]
    public float WeatherUpdatePeriod = 0.5f;
    [Rename("Weather Update Lerp Duration"),Suffix("Game Hour")]
    public float WeatherUpdateLerpDuration = 0.3f;
    [Rename("Processing Weather Update Period"),Suffix("Seconds")]
    public float ProcessingWeatherUpdatePeriod = 0.5f;
    public PrecipitationConfig PrecipitationConfig;
    public FogConfig fogConfig;
    public StormConfig stormConfig;
}
[System.Serializable]
public class PrecipitationConfig
{
    [Range(0f,1f)]
    public float PrecipitationThreshold;
    [Range(-60,60)]
    public float rainTemperatureThreshold;
    [Range(-60,60)]
    public float snowTemperatureThreshold;
    [Range(-1f,1f)]
    public float CloudinessInfluenceOnPrecipitation;

    [Header("Rain")]
    [Min(0)]
    public float RainMaxRotation;
    [Min(0)]
    public float RainSimulationSpeedMin;
    [Min(0)]
    public float RainSimulationSpeedMax;
    [Min(1)]
    public int RainParticleCountMin;
    [Min(1)]
    public int RainParticleCountMax;

    [Header("Snow")]
    [Min(0)]
    public float SnowSimulationSpeedMin;
    [Min(0)]
    public float SnowSimulationSpeedMax;
    [Min(1)]
    public int SnowParticleCountMin;
    [Min(1)]
    public int SnowParticleCountMax;
    public float SnowMaxVelocityX;
}

[System.Serializable]
public class FogConfig
{
    [Range(0f,1f)]
    public float FogThreshold;
   // [CurveRange(0f,-1f,24f,1f)]
	public AnimationCurve fogByTime;
    [Range(-1f,1f)]
    public float WindSpeedInfluenceOnFog;    
    [Range(-1f,1f)]
    public float PrecipitationInfluenceOnFog;
}

[System.Serializable]
public class StormConfig
{
    [Range(0f,1f)]
    public float StormThreshold;
    [Range(0f,1f)]
    public float MinLightningProbability;    
    [Range(0f,1f)]
    public float MaxLightningProbability;
    
    [Header("Influence")]
    [Range(-1f,1f)]
    public float WindSpeedInfluenceOnStorm;    
    [Range(-1f,1f)]
    public float PrecipitationInfluenceOnStorm;    
    [Range(-1f,1f)]
    public float CloudinessInfluenceOnStorm;
    [Header("Lightning")]
    public Color LightningGlobalLightColor;
    [Rename("Lightning Color Lerp Duration"),Suffix("Seconds")]
    public float LightningColorLerpDuration = 0.1f;

}