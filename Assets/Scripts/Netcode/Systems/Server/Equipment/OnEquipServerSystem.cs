using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;



public partial class EquipmentSystemGroup : ComponentSystemGroup
{
    
}


[UpdateInGroup(typeof(EquipmentSystemGroup),OrderLast = true)]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct OnEquipServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<PlayerContainers> playerContainersLookup;
    private BufferLookup<ItemBarData> barsLookup;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<EQOnEquip>();

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
        
        foreach ((RefRO<EQOnEquip> command, Entity entity) in
        SystemAPI.Query<RefRO<EQOnEquip>>().WithEntityAccess())
        {
            if(SystemAPI.Exists(command.ValueRO.connection))
            {
                Entity player = SystemAPI.GetComponent<LinkedCharacter>(command.ValueRO.connection).entity;
                int networkID =  SystemAPI.GetComponent<NetworkId>(command.ValueRO.connection).Value;
                var playerComp = SystemAPI.GetComponentRW<Player>(player);

                if(EQHelper.TryGetBufferIndex(slotsLookup,playerContainersLookup,player,command.ValueRO.slotPosition, out InventorySlot? inventorySlot, out int bufferIndex))
                {
                    if(ItemsAsset.instance.TryGetItem(inventorySlot.Value.itemId, out Garment item))
                    {
                        playerComp.ValueRW.speed += item.garmentStats.movementSpeed;
                        playerComp.ValueRW.aesthetic += item.garmentStats.aesthetic;
                        playerComp.ValueRW.armor += item.garmentStats.armor;
                        playerComp.ValueRW.waterResistance += item.garmentStats.waterResistance;
                        playerComp.ValueRW.insulation += item.garmentStats.insulation;
       
                        EQHelper.SendEvents(ref entityCommandBuffer,networkID, new EquipmentEvent
                        (
                            new EquipmentEventData(command.ValueRO.slotPosition.slotIndex,5),command.ValueRO.slotPosition.containerIndex)
                        );
                    }
                }
            }

            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
