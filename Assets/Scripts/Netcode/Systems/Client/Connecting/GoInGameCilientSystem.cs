using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct GoInGameCilientSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<MapIsLoaded>();
        state.RequireForUpdate<PlayerName>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId>().WithNone<NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();


    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<ReceiveRpcCommandRequest> request, RefRO<MapIsLoaded> map, Entity rpc) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<MapIsLoaded>>().WithEntityAccess())
        {
            entityCommandBuffer.DestroyEntity(rpc);

            foreach ((RefRO<NetworkId> networkId, Entity entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
            {
                entityCommandBuffer.AddComponent<NetworkStreamInGame>(entity);

                Entity rpcEntity = entityCommandBuffer.CreateEntity();

                var mapData = entityCommandBuffer.CreateEntity();
                entityCommandBuffer.AddComponent(mapData, new MapClientData() { widthInChunks = map.ValueRO.widthInChunks });
                ClientServerBootstrap.ClientWorld.GetExistingSystemManaged<MapLoadingClientSystem>().SetMapSettings(map.ValueRO);

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
