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
partial struct EquipmentManagmentServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
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
            EQHelper.SendEvents(ref entityCommandBuffer,events,networkID);
            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
    private void UpdateLookups(ref SystemState state)
    {
        slotsLookup.Update(ref state);
        playerContainersLookup.Update(ref state);
    }
    private EquipmentEvent[] MoveItem(ref SystemState state, ref EntityCommandBuffer entityCommandBuffer, EQMoveItem moveItem, Entity player)
    {
        var selectedSlot = SystemAPI.GetComponentRW<SelectedSlot>(player);
        var containerFrom = EQHelper.GetPlayerContainer(playerContainersLookup,player, selectedSlot.ValueRO.Position.containerIndex);
        var containerTo = EQHelper.GetPlayerContainer(playerContainersLookup, player, moveItem.to.containerIndex);

        if (!containerFrom.HasValue || !containerTo.HasValue) return null;
        return EQHelper.MoveBetweenContainers(slotsLookup , containerFrom.Value, containerTo.Value, moveItem.to.slotIndex, selectedSlot.ValueRO.Position.slotIndex, moveItem.value);
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
