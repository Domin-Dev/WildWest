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
partial struct DeselectionItemServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<PlayerContainers> playerContainersLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQDeselectItem>().WithAll<ReceiveRpcCommandRequest>();

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
        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, RefRO<EQDeselectItem> command, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<EQDeselectItem>>().WithEntityAccess())
        {
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(rpcCommandRequest.ValueRO.SourceConnection).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;
            var selectedSlot = SystemAPI.GetComponentRW<ContainerSettings>(player);

            if(!selectedSlot.ValueRO.Position.Compare(SlotPosition.NullSlot))
            {
                int slotIndex = EQHelper.ConvetSlotIndexToSelectedSlotIndex(selectedSlot.ValueRO.Position.slotIndex);

                if(EQHelper.TryGetBufferIndex(slotsLookup, playerContainersLookup, player, selectedSlot.ValueRO.Position, out InventorySlot? slot, out int index))
                {
                    EQHelper.TryGetBufferIndex(slotsLookup, playerContainersLookup, player, new SlotPosition(selectedSlot.ValueRO.Position.containerIndex, slotIndex), out InventorySlot? outSlot, out int bufferIndex);
                    int quantity = slot.Value.quantity;

                    if(!outSlot.HasValue || outSlot.Value.ItemId == slot.Value.ItemId)
                    {
                        var  container = EQHelper.GetPlayerContainer(playerContainersLookup, player, selectedSlot.ValueRO.Position.containerIndex);
                        if(container.HasValue)
                        {
                            Debug.Log("start!");
                            var events = EQHelper.MoveBetweenContainers(ref entityCommandBuffer, slotsLookup, rpcCommandRequest.ValueRO.SourceConnection, container.Value, container.Value, slotIndex, selectedSlot.ValueRO.Position.slotIndex, quantity, out int transferValue);
                            Debug.Log("start!2");

                            quantity -= transferValue;
                            EQHelper.SendEvents(ref entityCommandBuffer, events, networkID);
                            Debug.Log("start!3");

                        }
                    }

                    if (quantity > 0)
                    {
                        var items = EQHelper.FindSlotForItem(ref state, slotsLookup, playerContainersLookup, player, slot.Value.ItemId, quantity);
                        var events = EQHelper.MoveItems(ref entityCommandBuffer, slotsLookup, rpcCommandRequest.ValueRO.SourceConnection, playerContainersLookup, selectedSlot.ValueRO.Position, player, items, slot.Value.ItemId);
                        EQHelper.SendEvents(ref entityCommandBuffer, events, networkID);
                    }    
                }
                selectedSlot.ValueRW.Position = SlotPosition.NullSlot;
            }
            entityCommandBuffer.DestroyEntity(entity);
        }


        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
  
}
