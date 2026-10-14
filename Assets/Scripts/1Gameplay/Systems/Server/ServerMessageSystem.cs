using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(ServerTimeSystem))]
public partial struct ServerMessageSystem : ISystem
{
    private NativeParallelMultiHashMap<(Entity connection,uint messageIndex),(ClientMessageRPC message,NetworkTick tick)> MessageChunks;
    private EntityQuery cleanUpQuery;
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ClientMessageRPC, ReceiveRpcCommandRequest>();
        EntityQueryBuilder entityQueryBuilder2 = new EntityQueryBuilder(Allocator.Temp)
        .WithAll<MessageCleanUp,Execute>();

        cleanUpQuery = state.GetEntityQuery(entityQueryBuilder2);
        state.RequireAnyForUpdate(state.GetEntityQuery(entityQueryBuilder),cleanUpQuery);
        state.RequireForUpdate<NetworkTime>();

        MessageChunks = new NativeParallelMultiHashMap<(Entity connection, uint messageIndex), (ClientMessageRPC message,NetworkTick tick)>(32,Allocator.Persistent); 
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
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        foreach ((ClientMessageRPC message, ReceiveRpcCommandRequest receiveRpc, Entity entity) in
        SystemAPI.Query<ClientMessageRPC, ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            ecb.DestroyEntity(entity);
            var senderName = SystemAPI.GetComponent<PlayerName>(receiveRpc.SourceConnection).name;
            
            if(message.ChunkCount > 1)
            {
                var key = (receiveRpc.SourceConnection,message.MessageIndex);
                MessageChunks.Add(key,(message,currentTick));
                UnityEngine.Debug.Log("nowa wiadomosci server + " + message.ChunkCount + " " + message.ChunkIndex+ " ["+ message.MessageIndex + "] " + MessageChunks.CountValuesForKey(key));
                if(MessageChunks.CountValuesForKey(key) < message.ChunkCount)
                    continue;
            
                var tab = MessageChunks.GetValueArray(Allocator.Temp);
                ChatService.ServerProcessMessage(state.EntityManager,ecb,senderName,tab,receiveRpc.SourceConnection);
                tab.Dispose();
                MessageChunks.Remove(key);
            }
            else
                ChatService.ServerProcessMessage(state.EntityManager,ecb,senderName,message,receiveRpc.SourceConnection);
        }

        if(!cleanUpQuery.IsEmpty)
        {
            var cleanUpEntity = SystemAPI.GetSingletonEntity<MessageCleanUp>();
            var cleanUp = SystemAPI.GetSingleton<MessageCleanUp>();
            var valuesToRemove = new NativeList<((Entity connection,uint messageIndex) key,(ClientMessageRPC message,NetworkTick tick) value)>(Allocator.Temp);
            
            foreach(var message in MessageChunks)
            {
                if(currentTick.TicksSince(message.Value.tick) > cleanUp.CleanupPeriod)
                valuesToRemove.Add((message.Key,message.Value));   
            }       
            foreach(var value in valuesToRemove)
                MessageChunks.Remove(value.key,value.value);
   
            valuesToRemove.Dispose();
            ecb.RemoveComponent<Execute>(cleanUpEntity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    } 
}

