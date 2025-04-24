using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct TestCilientSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId>().WithNone<NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach((RefRO<NetworkId> networkId, Entity entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
        {
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(entity);
          //  Debug.Log("Connected! " + entity.ToString() + " " + networkId.ValueRO.Value);

            Entity rpcEntity = entityCommandBuffer.CreateEntity();
            PlayerName playerName = SystemAPI.GetSingleton<PlayerName>();

            entityCommandBuffer.AddComponent(rpcEntity,new GoInGameRequestRPC() { playerName = playerName.name });
            entityCommandBuffer.AddComponent<SendRpcCommandRequest>(rpcEntity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
