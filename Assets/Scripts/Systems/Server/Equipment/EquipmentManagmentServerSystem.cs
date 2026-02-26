using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;



[UpdateInGroup(typeof(EquipmentSystemGroup))]
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
            SlotPosition from =  command.ValueRO.from;
            if(command.ValueRO.from.IsNullSlot())
                from = SystemAPI.GetComponentRW<ContainerSettings>(player).ValueRO.Position;


            var containerFrom = EQHelper.GetPlayerContainer(playerContainersLookup, player, from.containerIndex);
            var containerTo = EQHelper.GetPlayerContainer(playerContainersLookup, player, command.ValueRO.to.containerIndex);

            List<EquipmentEvent> events = new List<EquipmentEvent>();
            if (containerFrom.HasValue && containerTo.HasValue)
            {
                var tab = EQHelper.MoveBetweenContainers(ref state, ref entityCommandBuffer,barsLookup, slotsLookup, rpcCommandRequest.ValueRO.SourceConnection,
                    containerFrom.Value, containerTo.Value, command.ValueRO.to.slotIndex, from.slotIndex, command.ValueRO.value);
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
