
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;



[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct MessageClientSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NewMessageServerRPC,ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((NewMessageServerRPC message, Entity entity) in
        SystemAPI.Query<NewMessageServerRPC>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        { 
            ChatManager.instance.PrintPlayerMessage(message.messageTime, message.message.ToString(), message.sender.ToString());
            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
