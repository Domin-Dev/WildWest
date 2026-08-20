using UnityEngine;
using NaughtyAttributes;


[CreateAssetMenu(fileName = "WeatherConfig", menuName = "GameAsset/ConfigFiles/WeatherConfig")]
public class WeatherConfig : ScriptableObject
{
    [Header("Weather Settings")]

    [Label("Weather Update Period (game hour)")]
    public float WeatherUpdatePeriod = 0.5f;
    [Label("Weather Update Lerp Duration (game hour)")]
    public float WeatherUpdateLerpDuration = 0.3f;
    [Range(-1f,1f)]
    public float CloudinessInfluenceOnPrecipitation;
    public PrecipitationConfig PrecipitationConfig;
}
[System.Serializable]
public class PrecipitationConfig
{
    [Range(0f,1f)]
    public float PrecipitationThreshold;
    [Min(0)]
    public float MaxParticleRotation;
    [Min(0)]
    public float ParticleSimulationSpeedMin;
    [Min(0)]
    public float ParticleSimulationSpeedMax;
    [Min(1)]
    public int ParticleCountMin;
    [Min(1)]
    public float ParticleCountMax;
}
