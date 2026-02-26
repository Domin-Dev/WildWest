
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;



[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct MessageClientSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ReceiveRpcCommandRequest>().WithAny<NewMessageServerRPC,PlayerJoinRPC,PlayerLeftRPC>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((NewMessageServerRPC message, Entity entity) in
        SystemAPI.Query<NewMessageServerRPC>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            if (message.senderIsServer)
            {
                ChatManager.instance.PrintServerMessage(message.messageTime, message.message.ToString());
            }
            else
            {
                ChatManager.instance.PrintPlayerMessage(message.messageTime, message.message.ToString(), message.sender.ToString());
            }
                entityCommandBuffer.DestroyEntity(entity);
        }

        foreach ((PlayerJoinRPC message, Entity entity) in
        SystemAPI.Query<PlayerJoinRPC>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            ChatManager.instance.PrintServerMessage(message.messageTime,$"{message.playerName} has joined the game.");
            entityCommandBuffer.DestroyEntity(entity);
        }

        foreach ((PlayerLeftRPC message, Entity entity) in
        SystemAPI.Query<PlayerLeftRPC>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            ChatManager.instance.PrintServerMessage(message.messageTime, $"{message.playerName} has left the game. {((NetworkStreamDisconnectReason)message.ReasonCode).ToString()}");
            entityCommandBuffer.DestroyEntity(entity);
        }


        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
