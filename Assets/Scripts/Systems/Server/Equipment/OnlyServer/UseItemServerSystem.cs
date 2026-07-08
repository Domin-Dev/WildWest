
using System.Diagnostics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;


public struct EQUseItem : IComponentData
{
    public SlotPosition slotPosition;
    public Entity networkEntity;
}


[UpdateInGroup(typeof(EquipmentSystemGroup),OrderFirst = true)]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct UseItemServerSystem : ISystem
{
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<EntityContainers> playerContainersLookup;
    private BufferLookup<LinkedContainers> linkedContainersLookup;


    private BufferLookup<LinkedEntityGroup> groupLookup;




    public void OnCreate(ref SystemState state)
    {
;
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<EQUseItem>();

        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        playerContainersLookup = SystemAPI.GetBufferLookup<EntityContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
        linkedContainersLookup = SystemAPI.GetBufferLookup<LinkedContainers>();
        groupLookup = SystemAPI.GetBufferLookup<LinkedEntityGroup>();
    }
    public void OnUpdate(ref SystemState state)
    {
        playerContainersLookup.Update(ref state);
        slotsLookup.Update(ref state);
        barsLookup.Update(ref state);
        linkedContainersLookup.Update(ref state);
        groupLookup.Update(ref state);

        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<EQUseItem> command, Entity entity) in
        SystemAPI.Query<RefRO<EQUseItem>>().WithEntityAccess())
        {
            int networkID = SystemAPI.GetComponent<NetworkId>(command.ValueRO.networkEntity).Value;
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(command.ValueRO.networkEntity).entity;
            if(EQHelper.TryGetPlayerContainer(playerContainersLookup,player,command.ValueRO.slotPosition.containerIndex,out var container))
            {
                EquipmentEvent[] events = null;
                if(EQHelper.TryGetBufferIndex(barsLookup,command.ValueRO.slotPosition.slotIndex,container.Value.entity,out var barValue,out int bufferIndex))
                {
                    float value = math.clamp(barValue.Value.value - 1,0,barValue.Value.maxValue);
                    barsLookup[container.Value.entity].ElementAt(bufferIndex).value = value;   
                    if(value == 0)
                    {
                        EQHelper.RemoveItem(command.ValueRO.slotPosition.slotIndex,entityCommandBuffer,groupLookup,linkedContainersLookup,barsLookup,slotsLookup,playerContainersLookup,container.Value.entity,player, out int itemID);
                        events = new []{ 
                            new EquipmentEvent(new EquipmentEventData(command.ValueRO.slotPosition.slotIndex, EquipementEventFlags.UpdateSlot),command.ValueRO.slotPosition.containerIndex),
                            new EquipmentEvent(new EquipmentEventData(command.ValueRO.slotPosition.slotIndex,EquipementEventFlags.BrokenItem,itemID),command.ValueRO.slotPosition.containerIndex)
                        };
                    }
                    else
                        events = new []{new EquipmentEvent(new EquipmentEventData(command.ValueRO.slotPosition.slotIndex,EquipementEventFlags.UpdateSlot),command.ValueRO.slotPosition.containerIndex)};
                }
                EQHelper.SendEvents(ref entityCommandBuffer, networkID,events);
            }
            entityCommandBuffer.DestroyEntity(entity);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
