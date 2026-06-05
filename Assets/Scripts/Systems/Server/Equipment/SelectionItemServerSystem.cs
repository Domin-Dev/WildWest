using System;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;



[UpdateInGroup(typeof(EquipmentSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct SelectionItemServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<EntityContainers> playerContainersLookup;
    private BufferLookup<LinkedContainers> linkedLookup;
    private ComponentLookup<ContainerComponent> containerComponents;



    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQSelectItem>().WithAll<ReceiveRpcCommandRequest>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        playerContainersLookup = SystemAPI.GetBufferLookup<EntityContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
        linkedLookup = SystemAPI.GetBufferLookup<LinkedContainers>();
        containerComponents = SystemAPI.GetComponentLookup<ContainerComponent>();
    }
    public void OnUpdate(ref SystemState state)
    {
        playerContainersLookup.Update(ref state);
        slotsLookup.Update(ref state);
        barsLookup.Update(ref state);
        linkedLookup.Update(ref state);
        containerComponents.Update(ref state);

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, RefRO<EQSelectItem> command, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<EQSelectItem>>().WithEntityAccess())
        {
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(rpcCommandRequest.ValueRO.SourceConnection).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;
            var selectedSlot = SystemAPI.GetComponentRO<ContainerSettings>(player);

            if (command.ValueRO.value > 0)
            {
                if (command.ValueRO.position.slotIndex >= 0) EQHelper.Deselection(ref state, ref entityCommandBuffer,linkedLookup,barsLookup, slotsLookup, playerContainersLookup, player, networkID, rpcCommandRequest.ValueRO.SourceConnection);
                SelectItem(ref state,entityCommandBuffer, player, command.ValueRO);
                if (command.ValueRO.position.slotIndex >= 0)
                {
                    EntityHelper.CreateEntityWithComponent(ref entityCommandBuffer, new EquipmentEvent
                        (new EquipmentEventData(command.ValueRO.position.slotIndex, 1), command.ValueRO.position.containerIndex, networkID));
                }
            }
            entityCommandBuffer.DestroyEntity(entity);
        }

        
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
    private void SelectItem(ref SystemState state,EntityCommandBuffer ecb, Entity player, EQSelectItem selectItem)
    {
        var container = EQHelper.GetContainer(playerContainersLookup,player, selectItem.position.containerIndex);

        if (!container.HasValue || SystemAPI.HasComponent<ServerContainer>(container.Value.entity)) return;
        if (selectItem.position.slotIndex >= 0 && EQHelper.TryGetBufferIndex(slotsLookup,selectItem.position.slotIndex, container.Value.entity, out int itemid, out int bufferIndex))
        {
            ref InventorySlot element = ref slotsLookup[container.Value.entity].ElementAt(bufferIndex);
            int newSlot = EQHelperClient.ConvetSlotIndexToSelectedSlotIndex(element.slot); 

            EQHelper.NewItemInTheSlot(ecb, containerComponents[container.Value.entity],element.slot,player,element);
            if (selectItem.value >= element.quantity)
            {
                if (EQHelper.TryGetBufferIndex(barsLookup,element.slot,container.Value.entity,out var barData,out int bIndex))
                {
                    barsLookup[container.Value.entity].ElementAt(bIndex).slot = newSlot;
                }

                if (EQHelper.TryGetBufferIndex(linkedLookup,element.slot,container.Value.entity,out var linked ,out int linkedIndex))
                {
                    linkedLookup[container.Value.entity].ElementAt(linkedIndex).slot = newSlot;
                }
                element.slot = newSlot;
            }
            else
            {
                int dif = element.quantity - selectItem.value;
                element.quantity = dif;

                slotsLookup[container.Value.entity].Add(new InventorySlot()
                {
                    itemId = element.itemId,
                    slot = newSlot,
                    quantity = selectItem.value,
                    wetness = element.wetness,
                    quality = element.quality,
                    color = element.color,
                });

                if (EQHelper.TryGetBufferIndex(barsLookup, element.slot, container.Value.entity, out var barData, out int bIndex))
                {
                    barsLookup[container.Value.entity].Add(new ItemBarData()
                    {
                        slot = newSlot,
                        maxValue = barData.Value.maxValue,
                        value = barData.Value.value,                       
                    });
                }
            }
        
        }
       
        var selectedSlot = SystemAPI.GetComponentRW<ContainerSettings>(player);

        if (selectItem.position.slotIndex >= 0)
        {
            selectItem.position.slotIndex = EQHelperClient.ConvetSlotIndexToSelectedSlotIndex(selectItem.position.slotIndex);
            selectedSlot.ValueRW.Position = selectItem.position;
        }
        else
        {
            selectedSlot.ValueRW.Position = selectItem.position;
        }
    }
}
