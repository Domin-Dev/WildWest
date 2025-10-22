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
using static UnityEngine.Rendering.DebugUI;

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
            var events = CombineItems(ref state, ref entityCommandBuffer, player, command.ValueRO);
            EntityHelper.SendEvents(ref entityCommandBuffer, events, networkID);
            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    EquipmentEvent[] CombineItems(ref SystemState state,ref EntityCommandBuffer ecb,Entity player,EQCombineAllItems command)
    {
        var container = EntityHelper.GetPlayerContainer(playerContainersLookup, player, command.position.containerIndex);
        if (!container.HasValue) return null; 

        if(EntityHelper.TryGetBufferIndex(slotsLookup, command.position.slotIndex,container.Value.entity, out int itemID, out int bufferIndex))
        {
            var slots = slotsLookup[container.Value.entity];
            var element = slots.ElementAt(bufferIndex);
            int maxStack = ItemsAsset.instance.GetStackMax(itemID);
            List<EquipmentEvent> list = new List<EquipmentEvent>();

            if(maxStack > element.quantity)
            {
                int gap = maxStack - element.quantity;

                for (int i = 0; i < slots.Length; i++)
                {
                    if (i == bufferIndex) continue;
                    ref var slot = ref slots.ElementAt(i);
                    if(slot.ItemId == itemID)
                    {
                        int transferValue = gap;
                        if (slot.quantity < gap)
                        {
                            transferValue = slot.quantity;
                            gap -= transferValue;
                        }
                        else
                            gap = 0;

                        var events = EntityHelper.MoveBetweenContainers(slotsLookup, container.Value, container.Value, command.position.slotIndex, slot.slot, transferValue,true);
                        list.AddRange(events);
                        if (gap <= 0) break;
                    }
                }
            }
            return list.ToArray();
        }
        return null;
    }

}
