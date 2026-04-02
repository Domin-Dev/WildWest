using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.NotBurstCompatible;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;


[UpdateInGroup(typeof(EquipmentSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GiveItemServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<PlayerContainers> playerContainersLookup;
    private BufferLookup<LinkedContainers> linkedContainersLookup;


    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();

        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQGiveItem>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        playerContainersLookup = SystemAPI.GetBufferLookup<PlayerContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
        linkedContainersLookup = SystemAPI.GetBufferLookup<LinkedContainers>();
    }
    public void OnUpdate(ref SystemState state)
    {
        playerContainersLookup.Update(ref state);
        slotsLookup.Update(ref state);
        barsLookup.Update(ref state);
        linkedContainersLookup.Update(ref state);

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        
        foreach ((RefRO<EQGiveItem> command, Entity entity) in
        SystemAPI.Query<RefRO<EQGiveItem>>().WithEntityAccess())
        {
            int networkID = SystemAPI.GetComponent<NetworkId>(command.ValueRO.networkEntity).Value;
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(command.ValueRO.networkEntity).entity;


            var slots = EQHelper.FindSlotForItem(ref state, slotsLookup, playerContainersLookup, player,command.ValueRO.item);
            var events = EQHelper.AddItems(ref state,entityCommandBuffer,ref entitiesReferences,linkedContainersLookup,barsLookup,slotsLookup, playerContainersLookup, player, slots, command.ValueRO);
           
            EQHelper.SendEvents(ref entityCommandBuffer, networkID, events);
            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
