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
    private BufferLookup<PlayerContainers> playerContainersLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQMoveItem,EQSelectItem>().WithAll<ReceiveRpcCommandRequest>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        playerContainersLookup = SystemAPI.GetBufferLookup<PlayerContainers>();
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
            EquipmentEvent[] events = MoveItem(ref state, ref entityCommandBuffer, command.ValueRO, player);
           
            
            if (events != null)
            {
                foreach (EquipmentEvent eventData in events)
                {
                    Debug.Log("<color=green> " + command.ValueRO.value);
                    eventData.SetNetworkID(networkID);
                    EntityHelper.CreateEntityWithComponent(ref entityCommandBuffer,eventData);
                }
            }
            entityCommandBuffer.DestroyEntity(entity);
        }

        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, RefRO<EQSelectItem> command, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<EQSelectItem>>().WithEntityAccess())
        {
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(rpcCommandRequest.ValueRO.SourceConnection).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;
            if (command.ValueRO.value > 0)
            {
                SelectItem(ref state,player, command.ValueRO);
                EntityHelper.CreateEntityWithComponent(ref entityCommandBuffer, new EquipmentEvent
                    (new EquipmentEventData(command.ValueRO.position.slotIndex,1), command.ValueRO.position.containerIndex,networkID));
            }
            entityCommandBuffer.DestroyEntity(entity);
        }






        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    private void UpdateLookups( ref SystemState state)
    {
        slotsLookup.Update(ref state);
        playerContainersLookup.Update(ref state);
    }
    private EquipmentEvent[] MoveItem(ref SystemState state, ref EntityCommandBuffer entityCommandBuffer,EQMoveItem moveItem, Entity player)
    {
        var selectedSlot = SystemAPI.GetComponentRW<SelectedSlot>(player);
        var container = GetPlayerContainer(player, selectedSlot.ValueRO.Position.containerIndex);
        if(!container.HasValue) return null;

        if (selectedSlot.ValueRO.Position.containerIndex == moveItem.to.containerIndex)
            return MoveInContainer(ref state, ref entityCommandBuffer,container.Value, moveItem, selectedSlot.ValueRO);
        else
        {

        }

        return null;
    }

    private EquipmentEvent[] MoveBetweenContainers(ref SystemState state, ref EntityCommandBuffer entityCommandBuffer, PlayerContainers containersFrom, PlayerContainers containersTo,EQMoveItem moveItem,SelectedSlot selected)
    {


    }
    private EquipmentEvent[] MoveInContainer(ref SystemState state, ref EntityCommandBuffer entityCommandBuffer, PlayerContainers container, EQMoveItem moveItem,SelectedSlot selectedSlot)
    {
        var slots = slotsLookup[container.entity];
        int number;
        TryGetBufferIndex(selectedSlot.Position.slotIndex, container.entity, out int itemIDFrom, out int fromIndex);
        TryGetBufferIndex(moveItem.to.slotIndex, container.entity,out int itemIDTo, out int toIndex);

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
                to = from.quantity;

            if (toIndex >= 0)
            {
                Debug.Log("adding " + to);
                slotsLookup[container.entity].ElementAt(toIndex).quantity += to;
            }
            else
                slotsLookup[container.entity].Add(new InventorySlot()
                {
                    slot = moveItem.to.slotIndex,
                    ItemId = itemIDFrom,
                    quantity = to,
                });

            if(number <= 0)
                slotsLookup[container.entity].RemoveAtSwapBack(fromIndex);
        }

        return new EquipmentEvent[]
        {
            new EquipmentEvent(new EquipmentEventData(moveItem.to.slotIndex, 1),container.index),
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
    public bool TryGetBufferIndex(int slotIndex, Entity container, out int itemID, out int bufferIndex)
    {
        var slots = slotsLookup[container];
        for (int j = 0; j < slots.Length; j++)
        {
            ref var slot = ref slots.ElementAt(j);
            if (slot.slot == slotIndex)
            {
                itemID = slot.ItemId;
                bufferIndex = j;
                return true;
            }
        }
        itemID = -1;
        bufferIndex = - 1;
        return false;
    }


    private void SelectItem(ref SystemState state,Entity player,EQSelectItem selectItem)
    {
        var container = GetPlayerContainer(player, selectItem.position.containerIndex);
        if (!container.HasValue) return;
        if (TryGetBufferIndex(selectItem.position.slotIndex, container.Value.entity, out int itemid, out int bufferIndex))
        {
            ref InventorySlot element = ref slotsLookup[container.Value.entity].ElementAt(bufferIndex);
            if (selectItem.value >= element.quantity)
            {
                element.slot = ConvetSlotIndexToSelectedSlotIndex(element.slot);
            }
            else
            {
                int dif = element.quantity - selectItem.value;
                element.quantity = dif;
                slotsLookup[container.Value.entity].Add(new InventorySlot() {
                    ItemId = element.ItemId,
                    slot = ConvetSlotIndexToSelectedSlotIndex(element.slot),
                    quantity = selectItem.value
                });
            }
        }
        var selectedSlot = SystemAPI.GetComponentRW<SelectedSlot>(player);
        selectItem.position.slotIndex = ConvetSlotIndexToSelectedSlotIndex(selectItem.position.slotIndex);
        selectedSlot.ValueRW.Position = selectItem.position;
    }

    private PlayerContainers? GetPlayerContainer(Entity player, int containerIndex)
    {
        if (!playerContainersLookup.TryGetBuffer(player, out var containers))
            return null;

        PlayerContainers? container = null;
        foreach (var item in containers)
        {
            if (item.index == containerIndex)
            {
                container = item;
                break;
            }
        }
        return container;
    }

    private int ConvetSlotIndexToSelectedSlotIndex(int slotIndex)
    {
        return -(slotIndex + 1);
    }
}
