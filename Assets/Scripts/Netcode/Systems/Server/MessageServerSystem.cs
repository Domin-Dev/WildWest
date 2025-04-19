using System;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct MessageServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NewMessageRPC,ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((NewMessageRPC requestRPC, ReceiveRpcCommandRequest receiveRpc, Entity entity) in
        SystemAPI.Query<NewMessageRPC, ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            entityCommandBuffer.DestroyEntity(entity);
            var sender = SystemAPI.GetComponent<PlayerName>(receiveRpc.SourceConnection).name;

            foreach ((RefRO<NetworkId> networkId, Entity obj) in SystemAPI.Query<RefRO<NetworkId>>().WithAll<NetworkStreamInGame>().WithEntityAccess())
            {
                Entity message = entityCommandBuffer.CreateEntity();

                entityCommandBuffer.AddComponent(message, new NewMessageServerRPC()
                {
                    message = requestRPC.message,
                    sender = sender,
                    messageTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
                entityCommandBuffer.AddComponent(message, new SendRpcCommandRequest()
                {
                    TargetConnection = obj,
                });
            }
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
