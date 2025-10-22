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
            if (command.ValueRO.value > 0)
            {
                SelectItem(ref state, player, command.ValueRO);
                EntityHelper.CreateEntityWithComponent(ref entityCommandBuffer, new EquipmentEvent
                    (new EquipmentEventData(command.ValueRO.position.slotIndex, 1), command.ValueRO.position.containerIndex, networkID));
            
            }
            entityCommandBuffer.DestroyEntity(entity);
        }

        
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
    private void SelectItem(ref SystemState state, Entity player, EQSelectItem selectItem)
    {
        var container = EntityHelper.GetPlayerContainer(playerContainersLookup,player, selectItem.position.containerIndex);

        if (!container.HasValue) return;
        if (EntityHelper.TryGetBufferIndex(slotsLookup,selectItem.position.slotIndex, container.Value.entity, out int itemid, out int bufferIndex))
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
                slotsLookup[container.Value.entity].Add(new InventorySlot()
                {
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

    private int ConvetSlotIndexToSelectedSlotIndex(int slotIndex)
    {
        return -(slotIndex + 1);
    }
}
