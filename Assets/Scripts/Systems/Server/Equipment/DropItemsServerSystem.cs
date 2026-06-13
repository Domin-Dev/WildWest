using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;



[UpdateInGroup(typeof(EquipmentSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct DropItemsServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<EntityContainers> playerContainersLookup;
    private BufferLookup<LinkedContainers> linkedLookup;
    private BufferLookup<PlayersNeedChunk> playerNeedChunkLookup;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQDropItem>().WithAll<ReceiveRpcCommandRequest>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        playerContainersLookup = SystemAPI.GetBufferLookup<EntityContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
        linkedLookup = SystemAPI.GetBufferLookup<LinkedContainers>();
        playerNeedChunkLookup = SystemAPI.GetBufferLookup<PlayersNeedChunk>();

        state.RequireForUpdate<Chunks>();
    }
    public void OnUpdate(ref SystemState state)
    {
        UpdateLookups(ref state);
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        SystemAPI.TryGetSingletonBuffer<LoadedChunks>(out var loadedChunks,true);
        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
        Chunks chunks = SystemAPI.GetSingleton<Chunks>();

        var currentTick = networkTime.ServerTick;


        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, RefRO<EQDropItem> command, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<EQDropItem>>().WithEntityAccess())
        {
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(rpcCommandRequest.ValueRO.SourceConnection).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;

            SlotPosition from =  command.ValueRO.position;
            PlayerInput playerInput = SystemAPI.GetComponent<PlayerInput>(player);
            LocalTransform localTransform = SystemAPI.GetComponent<LocalTransform>(player);
            float2 direction = playerInput.sightDirection - new float2(localTransform.Position.x, localTransform.Position.y);
            float randomValue = UnityEngine.Random.Range(0.2f,0.3f);
            direction = math.normalize(direction) * randomValue + new float2(localTransform.Position.x,localTransform.Position.y);
            
            ChunkManagementServerSystem.Map.settings.GetCorrectChunkAndPosition(direction,out int chunkIndex, out var correctPosition);
            direction = correctPosition;

             Debug.Log(chunkIndex + " drop chunk!!" + chunks.currentChunks.ContainsKey(chunkIndex));

            if(chunks.currentChunks.TryGetValue(chunkIndex,out Entity chunkEntity))
            {

                if(command.ValueRO.position.IsNullSlot())
                    from = SystemAPI.GetComponentRW<ContainerSettings>(player).ValueRO.Position;

                var containerFrom = EQHelper.GetContainer(playerContainersLookup, player, from.containerIndex);
                var containerTo = EQHelper.GetContainer(playerContainersLookup, chunkEntity, EquipmentConfig.chunkItems_ContainerIndex);

                List<EquipmentEvent> events = new List<EquipmentEvent>();
                if (containerFrom.HasValue && containerTo.HasValue && !SystemAPI.HasComponent<ServerContainer>(containerFrom.Value.entity) && EQHelper.TryGetBufferIndex(slotsLookup, from.slotIndex, containerFrom.Value.entity, out int itemID, out int index))
                {
                    int slotIndex = EQHelper.GetNextFreeSlotForItem(ref state,slotsLookup,playerContainersLookup,chunkEntity,EquipmentConfig.chunkItems_ContainerIndex);
                    int count = command.ValueRO.count >= 1 ? command.ValueRO.count : int.MaxValue;

                    var tab = EQHelper.MoveBetweenContainers(ref state, ref ecb,linkedLookup,barsLookup, slotsLookup, rpcCommandRequest.ValueRO.SourceConnection,player,
                        containerFrom.Value, containerTo.Value,slotIndex, from.slotIndex,count,moveBetweenObjects:true,serverMove:true);
                               
                    RPCHelper.SendEventsToClientsAndOwner<DropItemRPC>(new DropItemRPC(chunkIndex,slotIndex,direction,randomValue * 2f) ,ref state,playerNeedChunkLookup,loadedChunks,ecb,networkID,player,chunkIndex,currentTick,true);      
                    if (tab != null) events.AddRange(tab);
                }

                events.Add(new EquipmentEvent(new EquipmentEventData(EQHelperClient.GetNormalSlotIndex(from.slotIndex), 1), from.containerIndex));
                EQHelper.SendEvents(ref ecb,networkID,events.ToArray());
                ecb.DestroyEntity(entity);
            }
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose(); 
    }

    private void UpdateLookups(ref SystemState state)
    {
        slotsLookup.Update(ref state);
        playerContainersLookup.Update(ref state);
        barsLookup.Update(ref state);
        linkedLookup.Update(ref state);
        playerNeedChunkLookup.Update(ref state);
    }
}
