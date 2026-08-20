using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;
using System;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct GoInGameCilientSystem : ISystem
{
    public static Action<CurrentTime> OnStartTimer;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<StartDataRPC>();
        state.RequireForUpdate<PlayerName>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId>().WithNone<NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<ReceiveRpcCommandRequest> request, RefRO<StartDataRPC> startData, Entity rpc) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<StartDataRPC>>().WithEntityAccess())
        {
            entityCommandBuffer.DestroyEntity(rpc);
            EntityHelper.CreateEntityWithComponent(entityCommandBuffer,new MapSettings().LoadSetUp(startData.ValueRO.mapSetUp));
            EntityHelper.CreateEntityWithComponent(entityCommandBuffer,startData.ValueRO.currentTime);
            EntityHelper.CreateEntityWithComponent(entityCommandBuffer,new LocalWeather() { nextUpdate = startData.ValueRO.currentTime.StartTick});
            OnStartTimer?.Invoke(startData.ValueRO.currentTime);


            foreach ((RefRO<NetworkId> networkId, Entity entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
            {
                entityCommandBuffer.AddComponent<NetworkStreamInGame>(entity);
                Entity rpcEntity = entityCommandBuffer.CreateEntity();
                var mapData = entityCommandBuffer.CreateEntity();
                //.ClientWorld.GetExistingSystemManaged<MapLoadingClientSystem>().SetMapSettings(map.ValueRO);

                PlayerName playerName = SystemAPI.GetSingleton<PlayerName>();
                LocalPlayerLook look = SystemAPI.GetSingleton<LocalPlayerLook>();
                if(!GameInfo.instance.isHost)
                    GameInfo.instance.playerName = playerName.ToString();
                entityCommandBuffer.AddComponent(rpcEntity, new GoInGameRequestRPC()
                {
                    playerName = playerName.name,
                    characterLook = look.characterLook,
                });
                entityCommandBuffer.AddComponent<SendRpcCommandRequest>(rpcEntity);
            }
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
