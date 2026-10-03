using Unity.Entities;
using Unity.Mathematics;

public struct LocalWeather : IComponentData
{
    public float Temperature;
    public float Cloudiness;
    public float Precipitation; 
    public float FogIntensity;
    public float Storm;
    public float2 Wind;
    public float WindSpeed => math.length(Wind);
}