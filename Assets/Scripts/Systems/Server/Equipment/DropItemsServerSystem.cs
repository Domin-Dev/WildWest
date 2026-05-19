using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;



[UpdateInGroup(typeof(EquipmentSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct DropItemsServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<EntityContainers> playerContainersLookup;
    private BufferLookup<LinkedContainers> linkedLookup;

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
    }
    public void OnUpdate(ref SystemState state)
    {
        UpdateLookups(ref state);
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, RefRO<EQDropItem> command, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<EQDropItem>>().WithEntityAccess())
        {
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(rpcCommandRequest.ValueRO.SourceConnection).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;
            SlotPosition from =  command.ValueRO.position;
            Entity chunkEntity = SystemAPI.GetComponent<GhostChunk>(player).currentChunkEntity;


            if(command.ValueRO.position.IsNullSlot())
                from = SystemAPI.GetComponentRW<ContainerSettings>(player).ValueRO.Position;

            var containerFrom = EQHelper.GetPlayerContainer(playerContainersLookup, player, from.containerIndex);
            var containerTo = EQHelper.GetPlayerContainer(playerContainersLookup, chunkEntity, EquipmentConfig.chunkItems_ContainerIndex);

            List<EquipmentEvent> events = new List<EquipmentEvent>();
            if (containerFrom.HasValue && containerTo.HasValue && !SystemAPI.HasComponent<ServerContainer>(containerFrom.Value.entity))
            {
                var tab = EQHelper.MoveBetweenContainers(ref state, ref entityCommandBuffer,linkedLookup,barsLookup, slotsLookup, rpcCommandRequest.ValueRO.SourceConnection,player,
                    containerFrom.Value, containerTo.Value, EQHelper.GetNextFreeSlotForItem(ref state,slotsLookup,playerContainersLookup,chunkEntity,EquipmentConfig.chunkItems_ContainerIndex), from.slotIndex,moveBetweenObjects:true,serverMove:true);
                if (tab != null) events.AddRange(tab);
            }

            events.Add(new EquipmentEvent(new EquipmentEventData(EQHelperClient.GetNormalSlotIndex(from.slotIndex), 1), from.containerIndex));
            EQHelper.SendEvents(ref entityCommandBuffer,networkID,events.ToArray());
            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    private void UpdateLookups(ref SystemState state)
    {
        slotsLookup.Update(ref state);
        playerContainersLookup.Update(ref state);
        barsLookup.Update(ref state);
        linkedLookup.Update(ref state);
    }
}
