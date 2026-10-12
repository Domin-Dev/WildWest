using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(ServerTimeSystem))]
public partial struct ServerMessageSystem : ISystem
{
    private NativeParallelMultiHashMap<(Entity connection,uint messageIndex),(ClientMessageRPC message,NetworkTick tick)> MessageChunks;
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ClientMessageRPC, ReceiveRpcCommandRequest>();
        EntityQueryBuilder entityQueryBuilder2 = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<MessageSystemCleanUp,Execute>();
        
        state.RequireAnyForUpdate(state.GetEntityQuery(entityQueryBuilder),state.GetEntityQuery(entityQueryBuilder2));
        state.RequireForUpdate<ServerMessagesConfig>();
        state.RequireForUpdate<NetworkTime>();

        entityQueryBuilder2.Dispose();
        entityQueryBuilder.Dispose();
        MessageChunks = new NativeParallelMultiHashMap<(Entity connection, uint messageIndex), (ClientMessageRPC message,NetworkTick tick)>(32,Allocator.Persistent);
    
    }
    public void OnDestroy(ref SystemState state)
    {
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
                if(MessageChunks.CountValuesForKey(key) + 1 < message.ChunkCount)
                {
                    MessageChunks.Add(key,(message,currentTick));
                    continue;
                }

                var tab = MessageChunks.GetValueArray(Allocator.Temp);
                ChatService.ProcessMessage(state.EntityManager,ecb,senderName,tab,receiveRpc.SourceConnection);
                tab.Dispose();
                MessageChunks.Remove(key);
            }
            else
                ChatService.ProcessMessage(state.EntityManager,ecb,senderName,message,receiveRpc.SourceConnection);
        }
                        
        foreach ((var message,Entity entity) in
        SystemAPI.Query<MessageSystemCleanUp>().WithAll<Execute>().WithEntityAccess())
        {
            UnityEngine.Debug.Log("Cleanup!!!!");
           ecb.RemoveComponent<Execute>(entity);
        }
     
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    } 

   
}
