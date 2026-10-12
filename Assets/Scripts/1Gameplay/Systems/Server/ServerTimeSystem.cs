using Unity.Entities;
using Unity.NetCode;
public struct CurrentTime : IComponentData
{
    public double WorldTime => TimeService.GetWorldTime(Hour,Day);
    public float Hour;
    public TimeOfDay TimeOfDay;
    public float NextTimeOfDay;
    public int Day;
    public Season Season; 
    public NetworkTick StartTick;
    public float StartHour;

    public int HourInt => (int)Hour;
    public int MinuteInt => (int)((Hour - HourInt) * 60f);
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ServerTimeSystem : ISystem
{
    bool set;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CurrentTime>();
        state.RequireForUpdate<TimeConfig>();
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<NextWeatherUpdate>(); 
    }
    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var worldTime = SystemAPI.GetSingletonRW<CurrentTime>();
        var config = SystemAPI.GetSingleton<TimeConfig>();
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        set = false;

        foreach ((RefRO<SetTimeRequest> request, Entity entity) in SystemAPI.Query<RefRO<SetTimeRequest>>().WithEntityAccess())
        {
            set = true;
            TimeService.SetCurrentTime(networkTime.ServerTick,worldTime,request.ValueRO.newWorldtime);
            foreach ((RefRO<NetworkId> networkID, Entity connectionEntity) in SystemAPI.Query<RefRO<NetworkId>>().WithEntityAccess())
                RPCService.SendRpc(ecb,connectionEntity,new OnTimeSetRPC() { time = worldTime.ValueRO });
            SystemAPI.GetSingletonRW<NextWeatherUpdate>().ValueRW.tick = networkTime.ServerTick;
            ecb.DestroyEntity(entity);
        }

        if(!set)
            TimeService.UpdateCurrentTime(networkTime.ServerTick,worldTime,config,out int ticksSince,out bool nextDay,out bool nextSeason,out bool nextTimeOfDay);       
        ecb.Playback(state.EntityManager);
    }
}

