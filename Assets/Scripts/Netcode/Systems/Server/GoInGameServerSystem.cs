using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GoInGameRequestRPC>().WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        
        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, GoInGameRequestRPC requestRPC, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, GoInGameRequestRPC>().WithEntityAccess())
        {
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(rpcCommandRequest.ValueRO.SourceConnection);
            var networkId = state.EntityManager.GetComponentData<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;

            Entity character = entityCommandBuffer.Instantiate(SystemAPI.GetSingleton<EntitiesReferences>().characterEntity);

            entityCommandBuffer.SetComponent(character, LocalTransform.FromPosition(new float3(networkId * 0.5f, 0, 0)));
            entityCommandBuffer.SetComponent(character, new PlayerLook() { look = requestRPC.characterLook });
            entityCommandBuffer.AddComponent(character, new GhostOwner { NetworkId = networkId });
            entityCommandBuffer.SetComponent(character, new Player()
            {
                speed = 1f,
                playerName = requestRPC.playerName
            });


            entityCommandBuffer.SetComponent<Health>(character, new Health() { Max = 100, Value = 100 });
            entityCommandBuffer.SetComponent<Hunger>(character, new Hunger() { Max = 100, Value = 100 });
            entityCommandBuffer.SetComponent<Thirst>(character, new Thirst() { Max = 100, Value = 100 });



            entityCommandBuffer.AppendToBuffer(rpcCommandRequest.ValueRO.SourceConnection, new LinkedEntityGroup() { Value = character });

            Entity confirmation = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent<YouAreInGameRPC>(confirmation);
            entityCommandBuffer.AddComponent(confirmation, new SendRpcCommandRequest() { TargetConnection = rpcCommandRequest.ValueRO.SourceConnection });
            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
