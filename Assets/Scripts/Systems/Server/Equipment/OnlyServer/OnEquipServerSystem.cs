using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;


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
        slotsLookup.Update(ref state);
        playerContainersLookup.Update(ref state);
        barsLookup.Update(ref state);


        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RefRO<EQOnEquip> command, Entity entity) in
        SystemAPI.Query<RefRO<EQOnEquip>>().WithEntityAccess())
        {
            if(SystemAPI.Exists(command.ValueRO.player))
            {
                Debug.Log("Player outfit " + command.ValueRO.player);

                Entity connection = SystemAPI.GetComponent<PlayerSourceConnection>(command.ValueRO.player).value;
                int networkID =  SystemAPI.GetComponent<NetworkId>(connection).Value;
                var playerComp = SystemAPI.GetComponentRW<Player>(command.ValueRO.player);

                EQHelper.TryGetBufferIndex(slotsLookup,playerContainersLookup,command.ValueRO.player,command.ValueRO.slotPosition, out InventorySlot? inventorySlot, out int bufferIndex);  
                if(ItemsAsset.instance.TryGetItem(command.ValueRO.oldInventorySlot.itemId, out Garment item1))
                {
                    Equip(playerComp,item1,false);
                }

                if(inventorySlot.HasValue && ItemsAsset.instance.TryGetItem(inventorySlot.Value.itemId, out Garment item2))
                {
                    Equip(playerComp,item2,true);
                }

                EQHelper.SendEvents(ref entityCommandBuffer,networkID, new EquipmentEvent
                (
                    new EquipmentEventData(command.ValueRO.slotPosition.slotIndex,EquipementEventFlags.UpdateOutfit),command.ValueRO.slotPosition.containerIndex)
                );
                
            }
            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }



    private void Equip(RefRW<Player> playerComp, Garment item, bool addStats)
    {
        float x = addStats ? 1 : -1;
        playerComp.ValueRW.speed += x * item.garmentStats.movementSpeed;
        playerComp.ValueRW.aesthetic += x * item.garmentStats.aesthetic;
        playerComp.ValueRW.armor += x *  item.garmentStats.armor;
        playerComp.ValueRW.waterResistance += x * item.garmentStats.waterResistance;
        playerComp.ValueRW.insulation += x * item.garmentStats.insulation;
    }
}

