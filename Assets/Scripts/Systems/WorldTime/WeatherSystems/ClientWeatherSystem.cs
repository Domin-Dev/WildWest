using System;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateAfter(typeof(ClientTimeSystem))]
public partial struct ClientWeatherSystem : ISystem
{
    public static Action<CurrentTime> OnTimeUpdate;
    public static Action<CurrentTime> OnNextDay;
    public static Action<CurrentTime> OnNextSeason;
    public static Action<CurrentTime> OnNextTimeOfDay;
    private int tickRate;
    private NetworkTick lastTick;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CurrentTime>();
        state.RequireForUpdate<TimeConfig>();
        state.RequireForUpdate<NetworkTime>();
        tickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
    }
    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        NetworkTick currentTick = networkTime.ServerTick;
        if(currentTick.IsValid && (!lastTick.IsValid || currentTick.TicksSince(lastTick) >= tickRate))
        {
            
        }
    }
}

