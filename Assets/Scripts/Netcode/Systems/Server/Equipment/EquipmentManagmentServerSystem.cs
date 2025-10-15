using System;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct EquipmentManagmentServerSystem : ISystem
{


    private BufferLookup<InventorySlot> slotsLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<EQMoveItem>().WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
    }
    public void OnUpdate(ref SystemState state)
    {
        UpdateLookups(ref state);
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);


        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, EQMoveItem command, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, EQMoveItem>().WithEntityAccess())
        {

            Entity player = SystemAPI.GetComponent<LinkedCharacter>(rpcCommandRequest.ValueRO.SourceConnection).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;
            var containers = SystemAPI.GetBuffer<PlayerContainers>(player);

            EquipmentEvent[] events = MoveItem(ref state, ref entityCommandBuffer, ref containers, command);
           
            
            if (events != null)
            {
                foreach (EquipmentEvent eventData in events)
                {
                    eventData.SetNetworkID(networkID);
                    EntityHelper.CreateEntityWithComponent(ref entityCommandBuffer,eventData);
                }
            }
            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    private void UpdateLookups( ref SystemState state)
    {
        slotsLookup.Update(ref state);
    }
    private EquipmentEvent[] MoveItem(ref SystemState state, ref EntityCommandBuffer entityCommandBuffer,ref DynamicBuffer<PlayerContainers> containers,EQMoveItem moveItem)
    {
        PlayerContainers container = containers[0];
        foreach (var item in containers)
        {
            if (item.index == moveItem.from.containerIndex)
            {
                container = item;
            }
        }

        if (moveItem.from.containerIndex == moveItem.to.containerIndex)
            return MoveInContainer(ref state, ref entityCommandBuffer,container, moveItem);
        else
        {

        }

        return null;
    }
    private EquipmentEvent[] MoveInContainer(ref SystemState state, ref EntityCommandBuffer entityCommandBuffer, PlayerContainers container, EQMoveItem moveItem)
    {
        var slots = slotsLookup[container.entity];
        int number;
        int fromIndex = TryGetSlot(moveItem.from.slotIndex, container.entity, out int itemIDFrom);
        int toIndex = TryGetSlot(moveItem.to.slotIndex, container.entity,out int itemIDTo);


        if(fromIndex >= 0 && (itemIDFrom == itemIDTo || itemIDTo == -1))
        {
            ref InventorySlot from = ref slotsLookup[container.entity].ElementAt(fromIndex);
            int to;

            number = from.quantity - moveItem.value;
            if (number > 0)
            {
                from.quantity = number;
                to = moveItem.value;
            }
            else
            {
                to = from.quantity;
                slotsLookup[container.entity].RemoveAtSwapBack(fromIndex);
            }

            if (toIndex >= 0)
                slotsLookup[container.entity].ElementAt(fromIndex).quantity += to;
            else
                slotsLookup[container.entity].Add(new InventorySlot() { 
                    slot=moveItem.to.slotIndex,
                    ItemId=itemIDFrom,
                    quantity=to,
                });

        }

        return new EquipmentEvent[]
        {
            new EquipmentEvent(new EquipmentEventData(moveItem.to.slotIndex, 1),container.index),
            new EquipmentEvent(new EquipmentEventData(moveItem.from.slotIndex, 1),container.index),
        };
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


    public int TryGetSlot(int slotIndex, Entity container, out int itemID)
    {
        var slots = slotsLookup[container];
        for (int j = 0; j < slots.Length; j++)
        {
            ref var slot = ref slots.ElementAt(j);
            if (slot.slot == slotIndex)
            {
                itemID = slot.ItemId;
                return j;
            }
        }
        itemID = -1;
        return -1;
    }


}
