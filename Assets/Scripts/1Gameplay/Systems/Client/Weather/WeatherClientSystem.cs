using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateAfter(typeof(ClientTimeSystem))]
public partial struct WeatherClientSystem : ISystem
{
    public static Action<LocalWeather,CurrentTime> OnWeatherUpdate;
    private uint period;    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CurrentTime>();
        state.RequireForUpdate<TimeConfig>();
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<LocalWeather>();
        state.RequireForUpdate<MapSettings>();
        state.RequireForUpdate<NextWeatherUpdate>();

        period = (uint)(NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate * 60 * WorldConfig.TimeConfig.HourDuration * WorldConfig.WeatherConfig.WeatherUpdatePeriod);
    }
    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var localWeather = SystemAPI.GetSingletonRW<LocalWeather>();
        var nextWeatherUpdate = SystemAPI.GetSingletonRW<NextWeatherUpdate>();

        NetworkTick currentTick = networkTime.ServerTick;
        NetworkTick nextUpdate = nextWeatherUpdate.ValueRO.tick;

        if(currentTick.IsValid && nextUpdate.TicksSince(currentTick) <= 0)
        {
            var mapSettings = SystemAPI.GetSingleton<MapSettings>();
            var currentTime = SystemAPI.GetSingleton<CurrentTime>();
            float2 playerPosition = float2.zero;    

            foreach (RefRO<LocalToWorld> position in  SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<GhostOwnerIsLocal,Player>().WithNone<NewPlayerTag>())
            {
                playerPosition = new float2(position.ValueRO.Position.x,position.ValueRO.Position.y);
            }

            localWeather.ValueRW = WeatherService.GetWeather(mapSettings.seed,currentTime,playerPosition);

            UnityEngine.Debug.Log(" Temperature = " + localWeather.ValueRO.Temperature +
            " Wind = " + localWeather.ValueRO.WindSpeed + $" ({localWeather.ValueRO.Wind}) " + 
            " Clouds = " + localWeather.ValueRO.Cloudiness + 
            " Precipitation = " + localWeather.ValueRO.Precipitation + 
            " Fog = " + localWeather.ValueRO.FogIntensity +
            " Storm = " + localWeather.ValueRO.Storm);

            nextUpdate.Add(period);
            nextWeatherUpdate.ValueRW.tick = nextUpdate;

            OnWeatherUpdate?.Invoke(localWeather.ValueRO,currentTime);
        }
    }
}

