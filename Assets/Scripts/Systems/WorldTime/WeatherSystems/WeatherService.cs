using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public static class WeatherService
{ 
    private static float Noise(float2 position, int seed, float scale)
    {
        float offsetX = (seed * 0.12345f) % 100000f;
        float offsetY = (seed * 0.54321f) % 100000f;
        return math.clamp(Mathf.PerlinNoise(position.x * scale + offsetX, position.y * scale + offsetY),0f,1f);
    }
    public static LocalWeather GetWeather(int worldSeed,in CurrentTime currentTime, float2 position)
    {
        float2 windDir = GetWindDirection(position,currentTime.WorldTime,worldSeed);
        Vector2 weatherPosition = position - windDir * (float)currentTime.WorldTime * 0.1f;

        return new LocalWeather()
        {
            Wind = windDir,
        }; 
    }
    private static float2 GetWindDirection(float2 position,double worldTime,int worldSeed)
    {
        float2 windNoisePos = position * 0.1f + (float)worldTime * 0.3f;
        float2 windDir = ValueToDirection(Noise(windNoisePos, worldSeed + 1, 0.03f));
        float windSpeed = Noise(windNoisePos, worldSeed + 2, 0.03f) * WorldConfig.WeatherConfig.MaxWindSpeed;
    
        if (math.lengthsq(windDir) < 0.0001f)
            windDir = new float2(1f, 0f);
            
        windDir = math.normalize(windDir) * windSpeed;
        return windDir; 
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