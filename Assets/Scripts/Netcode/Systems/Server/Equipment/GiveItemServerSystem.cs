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
using UnityEngine.Experimental.Playables;
using UnityEngine.InputSystem.Processors;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GiveItemServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<PlayerContainers> playerContainersLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQGiveItem>();

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
        
        foreach ((RefRO<EQGiveItem> command, Entity entity) in
        SystemAPI.Query<RefRO<EQGiveItem>>().WithEntityAccess())
        {
            int networkID = SystemAPI.GetComponent<NetworkId>(command.ValueRO.networkEntity).Value;
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(command.ValueRO.networkEntity).entity;


            Debug.Log("Give!!");
            
            var slots = EQHelper.FindSlotForItem(ref state, slotsLookup, playerContainersLookup, player, command.ValueRO.itemID, command.ValueRO.quantity);
            var events = EQHelper.AddItems(slotsLookup, playerContainersLookup, player, slots, command.ValueRO.itemID);
            EQHelper.SendEvents(ref entityCommandBuffer, networkID, events);
            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
