using UnityEngine;
using NaughtyAttributes;


[CreateAssetMenu(fileName = "WeatherConfig", menuName = "GameAsset/ConfigFiles/WeatherConfig")]
public class WeatherConfig : ScriptableObject
{
    [Label("Weather Update Period (game hour)")]
    public float WeatherUpdatePeriod = 0.5f;
    [Label("Weather Update Lerp Duration (game hour)")]
    public float WeatherUpdateLerpDuration = 0.3f;
    public float MaxWindSpeed = 0.3f;
}