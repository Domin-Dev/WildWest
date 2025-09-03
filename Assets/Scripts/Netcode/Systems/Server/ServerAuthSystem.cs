using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct ServerAuthSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ClientHashRPC>().WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, RefRO<ClientHashRPC> commandRpc, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<ClientHashRPC>>().WithEntityAccess())
        {
            var salt = SystemAPI.GetComponentRO<ClientSalt>(rpcCommandRequest.ValueRO.SourceConnection);
         
            var saltHash = AuthUtils.GetSaltHash(SystemAPI.GetSingleton<ServerData>().hash, salt.ValueRO.salt);
            if(saltHash.CompareTo(commandRpc.ValueRO.hash) == 0)
            {
                Debug.Log("Witamy!!!");
                RPCHelper.SendRpc(ref entityCommandBuffer, rpcCommandRequest.ValueRO.SourceConnection, new AuthResponse() { success = true });
                entityCommandBuffer.AddComponent(rpcCommandRequest.ValueRO.SourceConnection, new AuthorizedClient());
            }
            else
            {
                Debug.Log("Zle Haslo!!!");
                RPCHelper.SendRpc(ref entityCommandBuffer, rpcCommandRequest.ValueRO.SourceConnection, new AuthResponse() { success = false });
                entityCommandBuffer.AddComponent<NetworkStreamRequestDisconnect>(rpcCommandRequest.ValueRO.SourceConnection , new NetworkStreamRequestDisconnect() { Reason = NetworkStreamDisconnectReason.AuthenticationFailure});
            }
            entityCommandBuffer.DestroyEntity(entity);           
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
