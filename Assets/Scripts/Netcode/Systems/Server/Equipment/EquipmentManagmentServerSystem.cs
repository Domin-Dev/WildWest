using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct EquipmentManagmentServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<PlayerContainers> playerContainersLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQMoveItem>().WithAll<ReceiveRpcCommandRequest>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        playerContainersLookup = SystemAPI.GetBufferLookup<PlayerContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
    }
    public void OnUpdate(ref SystemState state)
    {
        UpdateLookups(ref state);
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, RefRO<EQMoveItem> command, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<EQMoveItem>>().WithEntityAccess())
        {

            Entity player = SystemAPI.GetComponent<LinkedCharacter>(rpcCommandRequest.ValueRO.SourceConnection).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;

            var selectedSlot = SystemAPI.GetComponentRW<ContainerSettings>(player);
            var containerFrom = EQHelper.GetPlayerContainer(playerContainersLookup, player, selectedSlot.ValueRO.Position.containerIndex);
            var containerTo = EQHelper.GetPlayerContainer(playerContainersLookup, player, command.ValueRO.to.containerIndex);


            Debug.Log($"EQMoveItem from {selectedSlot.ValueRO.Position.ToString()} to {command.ValueRO.to.ToString()}");



            List<EquipmentEvent> events = new List<EquipmentEvent>();
            if (containerFrom.HasValue && containerTo.HasValue)
            {
                var tab = EQHelper.MoveBetweenContainers(ref state, ref entityCommandBuffer,barsLookup, slotsLookup, rpcCommandRequest.ValueRO.SourceConnection,
                    containerFrom.Value, containerTo.Value, command.ValueRO.to.slotIndex, selectedSlot.ValueRO.Position.slotIndex, command.ValueRO.value);
                if (tab != null) events.AddRange(tab);
            }
            events.Add(new EquipmentEvent(new EquipmentEventData(EQHelper.ConvetSlotIndexToSelectedSlotIndex(selectedSlot.ValueRO.Position.slotIndex), 1), selectedSlot.ValueRO.Position.containerIndex));

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
    }

    private bool SlotIsEmpty(Entity container, int slotIndex)
    {
        var slots = slotsLookup[container];
        for (int j = 0; j < slots.Length; j++)
        {
            InventorySlot slot = slots[j];
            if (slot.slot == slotIndex)
                return false;
        }
        return true;
    } 
}
