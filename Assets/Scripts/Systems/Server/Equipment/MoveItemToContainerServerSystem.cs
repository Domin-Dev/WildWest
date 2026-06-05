using System;
using System.Collections.Generic;
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



[UpdateInGroup(typeof(EquipmentSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct MoveItemToContainerServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<EntityContainers> playerContainersLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<LinkedContainers> linkedLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQMoveItemToContainer>().WithAll<ReceiveRpcCommandRequest>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        playerContainersLookup = SystemAPI.GetBufferLookup<EntityContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
        linkedLookup = SystemAPI.GetBufferLookup<LinkedContainers>();
    }
    public void OnUpdate(ref SystemState state)
    {
        playerContainersLookup.Update(ref state);
        slotsLookup.Update(ref state);
        barsLookup.Update(ref state);
        linkedLookup.Update(ref state);

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, RefRO<EQMoveItemToContainer> command, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<EQMoveItemToContainer>>().WithEntityAccess())
        {
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(rpcCommandRequest.ValueRO.SourceConnection).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;
            var selectedSlot = SystemAPI.GetComponentRW<ContainerSettings>(player);
            var containerFrom = EQHelper.GetContainer(playerContainersLookup, player,command.ValueRO.from.containerIndex);

            if(containerFrom.HasValue && !SystemAPI.HasComponent<ServerContainer>(containerFrom.Value.entity))
            {
                if (selectedSlot.ValueRO.targetContainer < 0)
                {
                    if (EQHelper.TryGetBufferIndex(slotsLookup, playerContainersLookup, player, command.ValueRO.from, out InventorySlot? slot, out int bufferindex))
                    {
                        List<int> containers = EQHelper.GetPlayerContainers(ref state, playerContainersLookup, player, slot.Value.itemId);
                        containers.Remove(command.ValueRO.from.containerIndex);
                        var itemsInContainer = EQHelper.TryGetAllItemsInContainer(slotsLookup,playerContainersLookup, player,command.ValueRO.from.containerIndex,slot.Value.itemId);

                        foreach (var item in itemsInContainer)
                        {
                            var items = EQHelper.FindSlotForItem(ref state, slotsLookup, playerContainersLookup, player, item, containers.ToArray());
                            var events = EQHelper.MoveItems(ref state, ref entityCommandBuffer,linkedLookup,barsLookup, slotsLookup, rpcCommandRequest.ValueRO.SourceConnection, playerContainersLookup,new SlotPosition(command.ValueRO.from.containerIndex,item.slot), player, items);
                            EQHelper.SendEvents(ref entityCommandBuffer,networkID, events);
                        }
                    }
                }
                else
                {

                }
            }

            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
  
}
