using System;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct SelectionItemServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<PlayerContainers> playerContainersLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQSelectItem>().WithAll<ReceiveRpcCommandRequest>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        playerContainersLookup = SystemAPI.GetBufferLookup<PlayerContainers>();
    }
    public void OnUpdate(ref SystemState state)
    {
        playerContainersLookup.Update(ref state);
        slotsLookup.Update(ref state);

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, RefRO<EQSelectItem> command, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<EQSelectItem>>().WithEntityAccess())
        {
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(rpcCommandRequest.ValueRO.SourceConnection).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;
            var selectedSlot = SystemAPI.GetComponentRO<ContainerSettings>(player);


            Debug.Log("kkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkk " + command.ValueRO.position);


            if (command.ValueRO.value > 0)
            {
               if (!EQHelper.BufferContains(slotsLookup, playerContainersLookup, player, selectedSlot.ValueRO.Position))
               {
                    if (command.ValueRO.position.slotIndex >= 0) EQHelper.Deselection(ref state, ref entityCommandBuffer, slotsLookup, playerContainersLookup, player, networkID, rpcCommandRequest.ValueRO.SourceConnection);
                    SelectItem(ref state, player, command.ValueRO);
                    if (command.ValueRO.position.slotIndex >= 0)
                    {
                        EntityHelper.CreateEntityWithComponent(ref entityCommandBuffer, new EquipmentEvent
                            (new EquipmentEventData(command.ValueRO.position.slotIndex, 1), command.ValueRO.position.containerIndex, networkID));
                    }
               }
            }
            entityCommandBuffer.DestroyEntity(entity);
        }

        
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
    private void SelectItem(ref SystemState state, Entity player, EQSelectItem selectItem)
    {
        var container = EQHelper.GetPlayerContainer(playerContainersLookup,player, selectItem.position.containerIndex);

        if (!container.HasValue) return;
        if (selectItem.position.slotIndex >= 0 && EQHelper.TryGetBufferIndex(slotsLookup,selectItem.position.slotIndex, container.Value.entity, out int itemid, out int bufferIndex))
        {
            ref InventorySlot element = ref slotsLookup[container.Value.entity].ElementAt(bufferIndex);
            if (selectItem.value >= element.quantity)
            {
                element.slot = EQHelper.ConvetSlotIndexToSelectedSlotIndex(element.slot);
            }
            else
            {
                int dif = element.quantity - selectItem.value;
                element.quantity = dif;
                Debug.Log("new element!");
                slotsLookup[container.Value.entity].Add(new InventorySlot()
                {
                    itemId = element.itemId,
                    slot = EQHelper.ConvetSlotIndexToSelectedSlotIndex(element.slot),
                    quantity = selectItem.value
                });
            }
        }
       
        var selectedSlot = SystemAPI.GetComponentRW<ContainerSettings>(player);

        if (selectItem.position.slotIndex >= 0)
        {
            selectItem.position.slotIndex = EQHelper.ConvetSlotIndexToSelectedSlotIndex(selectItem.position.slotIndex);
            selectedSlot.ValueRW.Position = selectItem.position;
        }
        else
        {
            selectedSlot.ValueRW.Position = selectItem.position;
        }
    }
}
