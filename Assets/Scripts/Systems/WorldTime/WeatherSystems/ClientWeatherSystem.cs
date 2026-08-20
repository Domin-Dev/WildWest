using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;


public struct LocalWeather : IComponentData
{
    public float Temperature;
    public float Humidity;
    public float Cloudiness;
    public float Precipitation; 
    public float2 Wind;
    public float WindSpeed => math.length(Wind);
    public NetworkTick nextUpdate;
    public bool IsRaining;
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateAfter(typeof(ClientTimeSystem))]
public partial struct ClientWeatherSystem : ISystem
{
    public static Action<LocalWeather,LocalWeather,CurrentTime> OnWeatherUpdate;
    private uint period;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CurrentTime>();
        state.RequireForUpdate<TimeConfig>();
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<LocalWeather>();
        state.RequireForUpdate<MapSettings>();
        period = (uint)(NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate * 60 * WorldConfig.TimeConfig.HourDuration * WorldConfig.WeatherConfig.WeatherUpdatePeriod);
    }
    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var localWeather = SystemAPI.GetSingletonRW<LocalWeather>();

        NetworkTick currentTick = networkTime.ServerTick;
        NetworkTick nextUpdate = localWeather.ValueRO.nextUpdate;

        if(currentTick.IsValid && nextUpdate.TicksSince(currentTick) <= 0)
        {
            var mapSettings = SystemAPI.GetSingleton<MapSettings>();
            var currentTime = SystemAPI.GetSingleton<CurrentTime>();
            float2 playerPosition = float2.zero;    

            foreach (RefRO<LocalToWorld> position in  SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<GhostOwnerIsLocal,Player>().WithNone<NewPlayerTag>())
            {
                playerPosition = new float2(position.ValueRO.Position.x,position.ValueRO.Position.y);
            }

            LocalWeather previousValue = localWeather.ValueRO;
            localWeather.ValueRW = WeatherService.GetWeather(mapSettings.seed,currentTime,playerPosition);

            UnityEngine.Debug.Log(" Temperature = " + localWeather.ValueRO.Temperature +
             " Wind = " + localWeather.ValueRO.WindSpeed + $" ({localWeather.ValueRO.Wind}) " + 
            " Clouds = " + localWeather.ValueRO.Cloudiness + 
            " Precipitation = " + localWeather.ValueRO.Precipitation);
            nextUpdate.Add(period);
            localWeather.ValueRW.nextUpdate = nextUpdate;
            
            OnWeatherUpdate?.Invoke(previousValue,localWeather.ValueRO,currentTime);
        }
    }
}

