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

    public override string ToString()
    {
        return $"Temperature : {Temperature.ToString("F1")}\n" +
       $"Wind : {WindSpeed.ToString("F1")} ({Wind.x.ToString("F2")},{Wind.y.ToString("F2")})\n" +
       $"Cloudiness = {Cloudiness.ToString("F2")}\n" +
       $"Precipitation = {Precipitation.ToString("F2")}\n" +
       $"Fog = {FogIntensity.ToString("F2")}\n" +
       $"Storm = {Storm.ToString("F2")}";
    }
}