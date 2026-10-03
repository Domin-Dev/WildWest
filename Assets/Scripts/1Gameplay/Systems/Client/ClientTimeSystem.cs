using System;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct ClientTimeSystem : ISystem
{
    public static Action<CurrentTime> OnTimeUpdate;
    public static Action<CurrentTime> OnNextDay;
    public static Action<CurrentTime> OnNextSeason;
    public static Action<CurrentTime> OnNextTimeOfDay;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CurrentTime>();
        state.RequireForUpdate<TimeConfig>();
        state.RequireForUpdate<NetworkTime>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var worldTime = SystemAPI.GetSingletonRW<CurrentTime>();
        var config = SystemAPI.GetSingleton<TimeConfig>();
        NetworkTick currentTick = networkTime.ServerTick;

        if(currentTick.IsValid)
        {
            TimeService.UpdateCurrentTime(currentTick,worldTime,config,out int ticksSince,out bool nextDay,out bool nextSeason,out bool nextTimeOfDay);   
            OnTimeUpdate?.Invoke(worldTime.ValueRO);
            
            if(nextDay)
            {
                OnNextDay?.Invoke(worldTime.ValueRO);
                if(nextSeason)
                    OnNextSeason?.Invoke(worldTime.ValueRO);
            }
            if(nextTimeOfDay)
                OnNextTimeOfDay?.Invoke(worldTime.ValueRO);        
        }
    }
}

