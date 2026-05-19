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
    private BufferLookup<EntityContainers> playerContainersLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<LinkedContainers> linkedLookup;
    private ComponentLookup<ContainerComponent> containerLookup;



    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQClear>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        playerContainersLookup = SystemAPI.GetBufferLookup<EntityContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
        linkedLookup = SystemAPI.GetBufferLookup<LinkedContainers>();
        containerLookup = SystemAPI.GetComponentLookup<ContainerComponent>();

    }
    public void OnUpdate(ref SystemState state)
    {
        playerContainersLookup.Update(ref state);
        slotsLookup.Update(ref state);
        barsLookup.Update(ref state);
        linkedLookup.Update(ref state);
        containerLookup.Update(ref state);

        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        
        foreach ((RefRO<EQClear> command, Entity entity) in
        SystemAPI.Query<RefRO<EQClear>>().WithEntityAccess())
        {
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(command.ValueRO.networkEntity).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(command.ValueRO.networkEntity).Value;
            EquipmentEvent[] events;

            if (command.ValueRO.containerIndex < 0)
                events = EQHelper.ClearAllContainer(ecb,containerLookup,linkedLookup,barsLookup,slotsLookup, playerContainersLookup, player);
            else
                events = EQHelper.ClearContainer(ecb,containerLookup,linkedLookup,barsLookup,slotsLookup, playerContainersLookup, player, command.ValueRO.containerIndex);        

            EQHelper.SendEvents(ref ecb, networkID, events);
            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}
