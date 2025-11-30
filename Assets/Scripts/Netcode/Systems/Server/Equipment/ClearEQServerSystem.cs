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
partial struct ClearEQServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<PlayerContainers> playerContainersLookup;
    private BufferLookup<ItemBarData> barsLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQClear>();

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
        
        foreach ((RefRO<EQClear> command, Entity entity) in
        SystemAPI.Query<RefRO<EQClear>>().WithEntityAccess())
        {
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(command.ValueRO.networkEntity).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(command.ValueRO.networkEntity).Value;
            EquipmentEvent[] events;

            if (command.ValueRO.containerIndex < 0)
                events = EQHelper.ClearAllContainer(barsLookup,slotsLookup, playerContainersLookup, player);
            else
                events = EQHelper.ClearContainer(barsLookup,slotsLookup, playerContainersLookup, player, command.ValueRO.containerIndex);        

            EQHelper.SendEvents(ref entityCommandBuffer, networkID, events);
            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
