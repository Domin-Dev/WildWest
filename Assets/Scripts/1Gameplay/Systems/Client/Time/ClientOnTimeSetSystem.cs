using System;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct ClientOnTimeSetSystem : ISystem
{
    public static Action<CurrentTime> OnTimeSet;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CurrentTime>();
        state.RequireForUpdate<NextWeatherUpdate>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var worldTime = SystemAPI.GetSingletonRW<CurrentTime>();
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

        foreach (var (rpc,entity) in SystemAPI.Query<RefRO<OnTimeSetRPC>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            worldTime.ValueRW = rpc.ValueRO.time;
            OnTimeSet?.Invoke(worldTime.ValueRO);
            SystemAPI.GetSingletonRW<NextWeatherUpdate>().ValueRW.tick = rpc.ValueRO.time.StartTick;
            ecb.DestroyEntity(entity);
        }
        ecb.Playback(state.EntityManager);
    }
}
 