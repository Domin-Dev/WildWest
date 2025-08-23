using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct VerifyingNewPlayersSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<PlayerVerificationRPC>().WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, PlayerVerificationRPC commandRpc, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, PlayerVerificationRPC>().WithEntityAccess())
        {
            PlayerSave playerSave = LoadSystem.LoadPlayerSave(commandRpc.playerName.ToString());
            bool isSave = playerSave != null;


            var answer = new AnswerPlayerVerificationRPC();
            answer.playerDataIsOnServer = isSave;
            if (isSave)
            {
                answer.characterLook = playerSave.characterLook;
            }

        RPCHelper.SendRpc(ref entityCommandBuffer, rpcCommandRequest.ValueRO.SourceConnection,answer);
            entityCommandBuffer.DestroyEntity(entity);           
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
