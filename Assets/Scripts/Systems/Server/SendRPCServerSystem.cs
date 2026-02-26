using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct SendRPCServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<RPCSendQueue>().WithNone<SendRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        foreach ((RefRO <RPCSendQueue> sendQueue, Entity entity) in
        SystemAPI.Query<RefRO<RPCSendQueue>>().WithNone<SendRpcCommandRequest>().WithEntityAccess())
        {
            if (currentTick.Equals(sendQueue.ValueRO.tick) || currentTick.IsNewerThan(sendQueue.ValueRO.tick))
            {
                entityCommandBuffer.AddComponent(entity, new SendRpcCommandRequest() { TargetConnection = sendQueue.ValueRO.target });
            }
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
