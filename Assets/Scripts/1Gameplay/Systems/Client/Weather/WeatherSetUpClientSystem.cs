using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateAfter(typeof(ClientTimeSystem))]
[UpdateBefore(typeof(WeatherClientSystem))]
public partial struct WeatherSetUpClientSystem   : ISystem
{
    public static Action<LocalWeather,CurrentTime> OnWeatherSetUp;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CurrentTime>();
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<MapSettings>(); 
        state.RequireForUpdate<NextWeatherUpdate>(); 
    }
    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var nextWeatherUpdate = SystemAPI.GetSingletonRW<NextWeatherUpdate>();
        var mapSettings = SystemAPI.GetSingleton<MapSettings>();
        var currentTime = SystemAPI.GetSingleton<CurrentTime>();
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        float2 playerPosition = float2.zero;    
        foreach (RefRO<LocalToWorld> position in  SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<GhostOwnerIsLocal,Player>().WithNone<NewPlayerTag>())
        {
            playerPosition = new float2(position.ValueRO.Position.x,position.ValueRO.Position.y);
        }

        var weather = WeatherService.GetWeather(mapSettings.seed,currentTime,playerPosition);
        EntityHelper.CreateEntityWithComponent(ecb,weather);
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        OnWeatherSetUp?.Invoke(weather,currentTime);
        
        state.Enabled = false;
    }
}

