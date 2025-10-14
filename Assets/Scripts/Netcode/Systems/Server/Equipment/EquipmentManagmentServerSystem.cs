using System;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct EquipmentManagmentServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        //state.RequireForUpdate<EntitiesReferences>();
        //EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
        //    .WithAll<EQMoveItem>().WithAll<ReceiveRpcCommandRequest>();
        //state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        //entityQueryBuilder.Dispose();
        i = 0;
    }
    int i;
    public void OnUpdate(ref SystemState state)
    {
        //i++;
        //Debug.Log("dziala!");

        //if (i % 50 == 0)
        //{
        //    Debug.Log("dziala!");
        //foreach ((DynamicBuffer<InventorySlot> buffer, Entity k) in
        //SystemAPI.Query<DynamicBuffer<InventorySlot>>().WithEntityAccess())
        //{
        //    buffer.RemoveAt(0);
        //    buffer.Add(new InventorySlot() { quantity = i });
        //}
        //}


        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        
        foreach ((RefRO<ReceiveRpcCommandRequest> rpcCommandRequest, EQMoveItem command, Entity entity) in
        SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, EQMoveItem>().WithEntityAccess())
        {
            Entity player = SystemAPI.GetComponent<LinkedCharacter>(rpcCommandRequest.ValueRO.SourceConnection).entity;
            int networkID = SystemAPI.GetComponent<NetworkId>(rpcCommandRequest.ValueRO.SourceConnection).Value;
            var containers = SystemAPI.GetBuffer<PlayerContainers>(player);

            EquipmentEvent[] events = MoveItem(ref state, ref entityCommandBuffer, ref containers, command.from, command.to);
            if (events != null)
            {
                foreach (EquipmentEvent eventData in events)
                {
                    eventData.SetNetworkID(networkID);
                    EntityHelper.CreateEntityWithComponent(ref entityCommandBuffer,eventData);
                }
            }

            ///
            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    private EquipmentEvent[] MoveItem(ref SystemState state, ref EntityCommandBuffer entityCommandBuffer,ref DynamicBuffer<PlayerContainers> containers,SlotPosition from, SlotPosition to)
    {
        if (from.containerIndex == to.containerIndex)
        {
           return MoveInContainer(ref state,ref entityCommandBuffer,from.containerIndex,to.slotIndex, from.slotIndex);
        }
        return null;
    }
    private EquipmentEvent[] MoveInContainer(ref SystemState state, ref EntityCommandBuffer entityCommandBuffer, int containerIndex, int slotIndexTo, int slotIndexFrom)
    {
        i += 100;
        foreach ((DynamicBuffer<InventorySlot> buffer, RefRO<ContainerComponent> container, Entity k) in
        SystemAPI.Query<DynamicBuffer<InventorySlot>, RefRO<ContainerComponent>>().WithEntityAccess())
        {
            var slots = buffer;
            if (container.ValueRO.containerIndex == containerIndex)
            {
                for (int j = 0; j < slots.Length; j++)
                {
                    InventorySlot slot = slots[j];
                    if (slots[j].slot == slotIndexFrom && slotIndexTo != 0)
                    {
                        slot.slot = slotIndexTo;
                        slots[j] = slot;
                    }
                }

                return new EquipmentEvent[]
                {
                    new EquipmentEvent(new EquipmentEventData(slotIndexTo, 1),containerIndex),
                    new EquipmentEvent(new EquipmentEventData(slotIndexFrom, 1),containerIndex),
                };
                break;
            }
        }




        // Debug.Log("wok!!" + slots.Length);
        //for (int i = 0; i < slots.Length; i++)
        //{
        //    var slot = slots[i];
        //    if (slot.slot == slotIndexFrom)
        //    {
        //        if (slotIndexTo == 0)
        //            slots.RemoveAtSwapBack(i);
        //       // slot.slot = slotIndexTo;
        //     //   slots.Add(slot);


        //        return new EquipmentEventData[]
        //        {
        //            new EquipmentEventData(new SlotPosition(containerIndex, slotIndexTo), 1),
        //            new EquipmentEventData(new SlotPosition(containerIndex, slotIndexFrom), 1)
        //        };
        //    }
        //}
        return null;
    }
}
