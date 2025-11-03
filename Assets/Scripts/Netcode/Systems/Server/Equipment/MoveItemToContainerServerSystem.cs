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

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]

partial struct MoveItemToContainerServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<PlayerContainers> playerContainersLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQMoveItemToContainer>().WithAll<ReceiveRpcCommandRequest>();

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
        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, RefRO<EQMoveItemToContainer> command, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<EQMoveItemToContainer>>().WithEntityAccess())
        {
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(rpcCommandRequest.ValueRO.SourceConnection).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;
            var selectedSlot = SystemAPI.GetComponentRW<ContainerSettings>(player);

            Debug.Log("kkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkk");
            if (selectedSlot.ValueRO.targetContainer < 0)
            {
                if (EQHelper.TryGetBufferIndex(slotsLookup, playerContainersLookup, player, command.ValueRO.from, out InventorySlot? slot, out int bufferindex))
                {
                    List<int> containers = EQHelper.GetPlayerContainers(ref state, playerContainersLookup, player, slot.Value.ItemId);
                    containers.Remove(command.ValueRO.from.containerIndex);
                    var itemsInContainer = EQHelper.TryGetAllItemsInContainer(slotsLookup,playerContainersLookup, player,command.ValueRO.from.containerIndex,slot.Value.ItemId);

                    foreach (var item in itemsInContainer)
                    {
                        var items = EQHelper.FindSlotForItem(ref state, slotsLookup, playerContainersLookup, player, item.ItemId, item.quantity, containers.ToArray());
                        var events = EQHelper.MoveItems(ref state, ref entityCommandBuffer, slotsLookup, rpcCommandRequest.ValueRO.SourceConnection, playerContainersLookup,new SlotPosition(command.ValueRO.from.containerIndex,item.slot), player, items, slot.Value.ItemId);
                        EQHelper.SendEvents(ref entityCommandBuffer, events, networkID);
                    }
                }
            }
            else
            {

            }
            Debug.Log("<Color=cyan>" + "move to container!");

            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
  
}
