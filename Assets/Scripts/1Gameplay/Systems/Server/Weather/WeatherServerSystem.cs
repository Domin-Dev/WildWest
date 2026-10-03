using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;


public struct NextWeatherUpdate : IComponentData
{
    public NetworkTick tick;
    public NetworkTick previousUpdate;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(ServerTimeSystem))]
public partial struct WeatherServerSystem : ISystem
{
    private uint period;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CurrentTime>();
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<MapSettings>(); 
        state.RequireForUpdate<NextWeatherUpdate>(); 
        period = (uint)(NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate * 60 * WorldConfig.TimeConfig.HourDuration * WorldConfig.WeatherConfig.WeatherUpdatePeriod);
    }
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var mapSettings = SystemAPI.GetSingleton<MapSettings>();
        var currentTime = SystemAPI.GetSingleton<CurrentTime>();
        var nextUpdate = SystemAPI.GetSingletonRW<NextWeatherUpdate>();
        
        NetworkTick currentTick = networkTime.ServerTick;
        float2 playerPosition = float2.zero;  

        if(!nextUpdate.ValueRO.tick.IsValid || nextUpdate.ValueRO.tick.TicksSince(currentTick) <= 0)
        {  
            foreach ((RefRO<LocalToWorld> position,RefRW<LocalWeather> localWeather) in  SystemAPI.Query<RefRO<LocalToWorld>,RefRW<LocalWeather>>().WithAll<Player>().WithNone<NewPlayerTag>())
            {
                playerPosition = new float2(position.ValueRO.Position.x,position.ValueRO.Position.y);
                localWeather.ValueRW = WeatherService.GetWeather(mapSettings.seed,currentTime,playerPosition);
            }
            
            nextUpdate.ValueRW.previousUpdate = nextUpdate.ValueRO.tick;
            if(!nextUpdate.ValueRO.tick.IsValid)
                nextUpdate.ValueRW.tick = currentTick;

            nextUpdate.ValueRW.tick.Add(period);
        }

        foreach ((RefRO<LocalToWorld> position,Entity player) in  SystemAPI.Query<RefRO<LocalToWorld>>().WithAll<Player>().WithNone<LocalWeather>().WithEntityAccess())
        {
            playerPosition = new float2(position.ValueRO.Position.x,position.ValueRO.Position.y);
            ecb.AddComponent(player,WeatherService.GetWeather(mapSettings.seed,currentTime,playerPosition));
        }
        ecb.Playback(state.EntityManager);
    }
}

