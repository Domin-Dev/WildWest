using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.NotBurstCompatible;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem.Processors;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct CombineItemsServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<PlayerContainers> playerContainersLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQCombineAllItems>().WithAll<ReceiveRpcCommandRequest>();

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
        
        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, RefRO<EQCombineAllItems> command, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<EQCombineAllItems>>().WithEntityAccess())
        {
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(rpcCommandRequest.ValueRO.SourceConnection).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;

            Debug.Log(";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;");
            
            var events = CombineItems(ref state, ref entityCommandBuffer, player,rpcCommandRequest.ValueRO.SourceConnection, command.ValueRO);
            EQHelper.SendEvents(ref entityCommandBuffer, events, networkID,command.ValueRO.position.slotIndex);
            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
    EquipmentEvent[] CombineItems(ref SystemState state,ref EntityCommandBuffer ecb,Entity player,Entity connection,EQCombineAllItems command)
    {
        var container = EQHelper.GetPlayerContainer(playerContainersLookup, player, command.position.containerIndex);
        if (!container.HasValue) return null; 

        if(EQHelper.TryGetBufferIndex(slotsLookup, command.position.slotIndex,container.Value.entity, out int itemID, out int bufferIndex))
        {
            var slots = slotsLookup[container.Value.entity];
            var element = slots.ElementAt(bufferIndex);
            int maxStack = ItemsAsset.instance.GetStackMax(itemID);
            List<EquipmentEvent> list = new List<EquipmentEvent>();
            List<(int from, int to, int amount)> moves = new();
            int foundMaxSlotPos = -1;
            int gap = maxStack - element.quantity;

            if (maxStack <= element.quantity) return null;            
            for (int i = 0; i < slots.Length; i++)
                {
                    if (i == bufferIndex) continue;
                    ref var slot = ref slots.ElementAt(i);
                    if(slot.ItemId == itemID)
                    {
                        if (slot.quantity == maxStack)
                        {
                            foundMaxSlotPos = slot.slot;
                            continue;
                        }

                        int transferValue = gap;
                        if (slot.quantity < gap)
                        {
                            transferValue = slot.quantity;
                            gap -= transferValue;
                        }
                        else
                            gap = 0;

                        moves.Add(new(slot.slot, command.position.slotIndex, transferValue));
                        if (gap <= 0)
                            break;                     
                    }
                }
          
            if(gap > 0 && foundMaxSlotPos >= 0)
                moves.Add(new(foundMaxSlotPos, command.position.slotIndex, gap));

            foreach (var item in moves)
                list.AddRange(EQHelper.MoveBetweenContainers(ref state,ref ecb, slotsLookup, connection, container.Value, container.Value, item.to, item.from, item.amount, true));

            return list.ToArray();
        }
        return null;
    }
}
