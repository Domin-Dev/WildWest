using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;


// [UpdateInGroup(typeof(RpcCommandRequestSystemGroup),OrderFirst = true)]
// partial struct RPCProcessingSystem : ISystem
//{
//     public void OnCreate(ref SystemState state)
//     {
//         state.RequireForUpdate<EntitiesReferences>();
//         // EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
//         //     .WithAll<NewItemInHandRPC>().WithNone<EventToDo,ReceiveRpcCommandRequest>();
//         // state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
//         // entityQueryBuilder.Dispose();
//     }
//     public void OnUpdate(ref SystemState state)
//     {
//         EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

//         foreach ((RefRO<NewItemInHandRPC> rpc, Entity entity) in
//         SystemAPI.Query<RefRO<NewItemInHandRPC>>().WithEntityAccess())
//         {
//             var newRPC = new NewAmmoSelectedRPC()
//             {
//                 ammoID = 140,
//                 weaponID = 138,
//                 networkID = rpc.ValueRO.networkID,
//                 tick = rpc.ValueRO.tick
//             };
//             Debug.Log("Print ");

//             if(state.EntityManager.HasComponent<SendRpcCommandRequest>(entity))
//             {
//                 var connection = state.EntityManager.GetComponentData<SendRpcCommandRequest>(entity);
//                 RPCHelper.SendRpc(state.EntityManager,connection.TargetConnection,newRPC);
//             }
//             else
//             {
//                 state.EntityManager.AddComponent<EventToDo>(entity);
//                 EntityHelper.CreateEntityWithComponent(state.EntityManager,newRPC);
//             }
//         }

//         // foreach ((RefRO<NewAmmoSelectedRPC> rpc, Entity entity) in
//         // SystemAPI.Query<RefRO<NewAmmoSelectedRPC>>().WithNone<EventToDo,ReceiveRpcCommandRequest>().WithEntityAccess())
//         // {
//         //     if(state.EntityManager.HasComponent<SendRpcCommandRequest>(entity))
//         //     {
                
//         //     }
//         //     else
//         //     {
//         //         state.EntityManager.AddComponent<EventToDo>(entity);
//         //     }
//         // }




//         entityCommandBuffer.Playback(state.EntityManager);
//         entityCommandBuffer.Dispose();
//     }
// }
