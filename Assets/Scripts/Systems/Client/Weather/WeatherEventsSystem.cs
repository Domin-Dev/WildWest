using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateAfter(typeof(ClientTimeSystem))]
public partial struct WeatherEventsSystem : ISystem
{
    public static Action<float2> OnLightning; 
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<LightningRPC>().WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    } 
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        foreach ((RefRO<LightningRPC> rpc, Entity entity) in  SystemAPI.Query<RefRO<LightningRPC>>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            OnLightning?.Invoke(rpc.ValueRO.position);      
            ecb.DestroyEntity(entity);   
        }
        ecb.Playback(state.EntityManager);
    }
}

