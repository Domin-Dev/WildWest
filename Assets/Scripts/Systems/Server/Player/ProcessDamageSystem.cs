using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;



[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[RequireMatchingQueriesForUpdate]
partial struct ProcessDamageSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach ((RefRW<LocalTransform> transform ,RefRW<GhostChunk> ghostChunk, DynamicBuffer<DamageBuffer> damageBuffer, RefRW<Health> health, Entity entity) in
        SystemAPI.Query<RefRW<LocalTransform>,RefRW<GhostChunk>, DynamicBuffer<DamageBuffer>,RefRW<Health>>().WithEntityAccess())
        {
            if(damageBuffer.IsEmpty) continue;
            int sum = 0;
            bool isPlayer = SystemAPI.HasComponent<PlayerSourceConnection>(entity);
            Entity connection = isPlayer ? SystemAPI.GetComponent<PlayerSourceConnection>(entity).value : Entity.Null;

            foreach(var element in damageBuffer)
                sum += element.value;
            health.ValueRW.Value = Mathf.Clamp(health.ValueRO.Value - sum,0,health.ValueRO.Max); 

            if(health.ValueRO.Value == 0)
            {
                transform.ValueRW.Position = float3.zero;
                health.ValueRW.Value = health.ValueRO.Max;
                ghostChunk.ValueRW.lastPosition = GhostChunk.incorrectPosition;
                if(isPlayer)
                    RPCHelper.SendMessageToClients(ecb,SystemAPI.GetComponent<PlayerName>(connection).name.ToString() + " has died");
            }

            if(isPlayer)
            {
                RPCHelper.SendRpc(ecb,connection, new LifeStatsChangedRPC()); 
            }

            damageBuffer.Clear();       
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }



}   