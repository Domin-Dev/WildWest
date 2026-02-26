using System;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;


[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct EventsServerSystem : ISystem
{
    private const int maxChunkEventsBufferPreClient = 50;
    private const int cutoffBorder = 100000;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EquipmentEvent>();
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach ((RefRO<EquipmentEvent> eventData,Entity entity)
        in SystemAPI.Query<RefRO<EquipmentEvent>>().WithEntityAccess())
        {
            CreateNewEquipmentEvent(ref state,entityCommandBuffer,eventData.ValueRO);
            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
    public void CreateNewEquipmentEvent(ref SystemState state,EntityCommandBuffer ecb,EquipmentEvent equipmentEvent)
    {
        foreach ((DynamicBuffer<EquipmentEventBuffer> events, RefRO<GhostOwner> ghostOwner, RefRO<ContainerComponent> container, RefRW<ServerEquipmentEventCounter> counter, RefRO<EquipmentEventCounter> clientCounter, Entity entity)
        in SystemAPI.Query<DynamicBuffer<EquipmentEventBuffer>, RefRO<GhostOwner>, RefRO<ContainerComponent>, RefRW<ServerEquipmentEventCounter>, RefRO<EquipmentEventCounter>>().WithEntityAccess())
        {
            if (ghostOwner.ValueRO.NetworkId == equipmentEvent.networkID && container.ValueRO.containerIndex == equipmentEvent.containerIndex)
            {
                EquipmentEventBuffer equipmentEventBuffer = new EquipmentEventBuffer(equipmentEvent.data, counter.ValueRO.index);
                events.Add(equipmentEventBuffer);
                counter.ValueRW.index++;
                ClearBuffer(events, clientCounter.ValueRO.index);
                ecb.SetComponentEnabled<ToSave>(entity,true);
            }
        }
    }
    private void ClearBuffer<T>(DynamicBuffer<T> buffer, uint clientCounter) where T : unmanaged, IBufferElementData, IIndexed
    {
        if (buffer.Length > maxChunkEventsBufferPreClient)
        {
            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                long dis = Math.Abs((long)buffer[i].GetIndex() - (long)clientCounter);
                if ((buffer[i].GetIndex() < clientCounter && dis < cutoffBorder)
                  || (buffer[i].GetIndex() > clientCounter && dis > cutoffBorder))
                {
                    buffer.RemoveAt(i);
                }
            }
        }
    }
}