using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;



//[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
//partial struct StartWorldSetupCilientSystem : ISystem
//{
//    public void OnCreate(ref SystemState state)
//    {
//        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
//        .WithAll<NetworkId>().WithNone<NetworkStreamInGame>();
//        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
//        entityQueryBuilder.Dispose();
//    }

//    public void OnUpdate(ref SystemState state)
//    {
//        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

//        foreach ((RefRO<LoadMap> loadMap, Entity entity) in SystemAPI.Query<RefRO<LoadMap>>().WithEntityAccess())
//        {

//            Entity rpcEntity = entityCommandBuffer.CreateEntity();
//            PlayerName playerName = SystemAPI.GetSingleton<PlayerName>();

//            entityCommandBuffer.AddComponent(rpcEntity, new NewPlayerJoinRPC()
//            {
//                playerName = playerName.name
//            });
//            entityCommandBuffer.AddComponent<SendRpcCommandRequest>(rpcEntity);
//            entityCommandBuffer.DestroyEntity(entity);
//        }

//        entityCommandBuffer.Playback(state.EntityManager);
//        entityCommandBuffer.Dispose();
//    }
//}
