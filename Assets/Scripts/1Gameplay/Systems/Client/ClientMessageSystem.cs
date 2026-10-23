
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;



[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct ClientMessageSystem : ISystem
{   
    private NativeParallelMultiHashMap<uint,(ServerMessageRPC message,NetworkTick tick)> MessageChunks;
    private EntityQuery cleanUpQuery;
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ReceiveRpcCommandRequest>().WithAny<ServerMessageRPC,PlayerJoinRPC,PlayerLeftRPC>();
        EntityQueryBuilder entityQueryBuilder2 = new EntityQueryBuilder(Allocator.Temp)
        .WithAll<MessageCleanUp,Execute>();

        cleanUpQuery = state.GetEntityQuery(entityQueryBuilder2);
        state.RequireAnyForUpdate(state.GetEntityQuery(entityQueryBuilder),cleanUpQuery);
        state.RequireForUpdate<NetworkTime>();

        MessageChunks = new NativeParallelMultiHashMap<uint, (ServerMessageRPC message,NetworkTick tick)>(16,Allocator.Persistent); 
        entityQueryBuilder.Dispose();
        entityQueryBuilder2.Dispose();
    }
    public void OnDestroy(ref SystemState state)
    {        
        state.CompleteDependency();
        MessageChunks.Dispose();        
    }
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        foreach ((ServerMessageRPC message,ReceiveRpcCommandRequest receiveRpc, Entity entity) in
        SystemAPI.Query<ServerMessageRPC,ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);
            
            if(message.ChunkCount > 1)
            {
                var key = message.MessageIndex;
                MessageChunks.Add(key,(message,currentTick));
                UnityEngine.Debug.Log("nowa wiadomosci + " + message.ChunkCount + " " + message.ChunkIndex+ " ["+ message.MessageIndex + "] " + MessageChunks.CountValuesForKey(key));
                if(MessageChunks.CountValuesForKey(key) < message.ChunkCount)
                    continue;

                var tab = MessageChunks.GetValueArray(Allocator.Temp);
                ChatService.ClientProcessMessage(tab);
                tab.Dispose();
                MessageChunks.Remove(key);
            }
            else
                ChatService.ClientProcessMessage(message);
        }
        
        foreach ((PlayerJoinRPC message, Entity entity) in
        SystemAPI.Query<PlayerJoinRPC>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            Chat.Instance.PrintServerMessage(message.messageTime,$"{message.playerName} has joined the game.");
            ecb.DestroyEntity(entity);
        }

        foreach ((PlayerLeftRPC message, Entity entity) in
        SystemAPI.Query<PlayerLeftRPC>().WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            Chat.Instance.PrintServerMessage(message.messageTime, $"{message.playerName} has left the game. {((NetworkStreamDisconnectReason)message.ReasonCode).ToString()}");
            ecb.DestroyEntity(entity);
        }

        if(!cleanUpQuery.IsEmpty)
        {
            var cleanUpEntity = SystemAPI.GetSingletonEntity<MessageCleanUp>();
            var cleanUp = SystemAPI.GetSingleton<MessageCleanUp>();
            var valuesToRemove = new NativeList<(uint messageIndex,(ServerMessageRPC message,NetworkTick tick) value)>(Allocator.Temp);

            foreach(var message in MessageChunks)
            {
                if(currentTick.TicksSince(message.Value.tick) > cleanUp.CleanupPeriod)
                   valuesToRemove.Add((message.Key,message.Value));   
            }       
            foreach(var value in valuesToRemove)
                MessageChunks.Remove(value.messageIndex,value.value);

            valuesToRemove.Dispose();
            ecb.RemoveComponent<Execute>(cleanUpEntity);
        }
 
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
