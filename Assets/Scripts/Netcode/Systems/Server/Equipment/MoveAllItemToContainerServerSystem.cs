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
partial struct MoveAllItemToContainerServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<PlayerContainers> playerContainersLookup;
    private BufferLookup<ItemBarData> barsLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQMoveAllItemsToContainer>().WithAll<ReceiveRpcCommandRequest>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        playerContainersLookup = SystemAPI.GetBufferLookup<PlayerContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
    }
    public void OnUpdate(ref SystemState state)
    {
        playerContainersLookup.Update(ref state);
        slotsLookup.Update(ref state);
        barsLookup.Update(ref state);

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, RefRO<EQMoveAllItemsToContainer> command, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<EQMoveAllItemsToContainer>>().WithEntityAccess())
        {
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(rpcCommandRequest.ValueRO.SourceConnection).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;
            var selectedSlot = SystemAPI.GetComponentRW<ContainerSettings>(player);



             Debug.Log("kkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkk");
            if (selectedSlot.ValueRO.targetContainer < 0)
            {
                if (EQHelper.TryGetBufferIndex(slotsLookup, playerContainersLookup, player, command.ValueRO.from, out InventorySlot? slot, out int bufferindex))
                {
                    List<int> containers = EQHelper.GetPlayerContainers(ref state, playerContainersLookup, player, slot.Value.itemId);
                    containers.Remove(command.ValueRO.from.containerIndex);
                    var items = EQHelper.FindSlotForItem(ref state, slotsLookup, playerContainersLookup, player,slot.Value, containers.ToArray());
                    var events = EQHelper.MoveItems(ref state,ref entityCommandBuffer,barsLookup, slotsLookup, rpcCommandRequest.ValueRO.SourceConnection, playerContainersLookup, command.ValueRO.from, player, items);
                    EQHelper.SendEvents(ref entityCommandBuffer, networkID, events);
                }
            }
            else
            {

            }
            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
  
}
