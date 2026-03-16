using System;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine.Diagnostics;


[UpdateInGroup(typeof(SimulationSystemGroup),OrderLast = true)]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct EventsServerSystem : ISystem
{
    private const int maxChunkEventsBufferPreClient = 50;
    private const int cutoffBorder = 100000;


    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<PlayerContainers> containersLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<PlayersNeedChunk> playerNeedChunkLookup;


    private NetworkTick tick;
    private DynamicBuffer<LoadedChunks> loadedChunks;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EquipmentEvent>();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        containersLookup = SystemAPI.GetBufferLookup<PlayerContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
        playerNeedChunkLookup = SystemAPI.GetBufferLookup<PlayersNeedChunk>();
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        slotsLookup.Update(ref state);
        containersLookup.Update(ref state);
        barsLookup.Update(ref state);
        playerNeedChunkLookup.Update(ref state);

        loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(true);
        tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;


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
        foreach ((DynamicBuffer<EquipmentEventBuffer> events, RefRO<GhostOwner> ghostOwner, RefRO<ContainerComponent> container, RefRW<ServerEquipmentEventCounter> counter, RefRO<EquipmentEventCounter> clientCounter,RefRO<PlayerContainer> playerContainer, Entity entity)
        in SystemAPI.Query<DynamicBuffer<EquipmentEventBuffer>, RefRO<GhostOwner>, RefRO<ContainerComponent>, RefRW<ServerEquipmentEventCounter>, RefRO<EquipmentEventCounter>,RefRO<PlayerContainer>>().WithEntityAccess())
        {
            if (ghostOwner.ValueRO.NetworkId == equipmentEvent.networkID && container.ValueRO.containerIndex == equipmentEvent.containerIndex)
            {
                EquipmentEventBuffer equipmentEventBuffer = new EquipmentEventBuffer(equipmentEvent.data, counter.ValueRO.index);
                events.Add(equipmentEventBuffer);
                counter.ValueRW.index++;
                ClearBuffer(events, clientCounter.ValueRO.index);
                ecb.SetComponentEnabled<ToSave>(entity,true);
                if(equipmentEvent.containerIndex == EquipmentConfig.hotBar_ContainerIndex)
                {
                    var input = SystemAPI.GetComponentRO<PlayerInputSync>(playerContainer.ValueRO.player);
                    if(input.ValueRO.slotInHand == equipmentEvent.slotPosition.slotIndex)
                    {
                        Debug.Log("<Color=cyan> dziala update!!!");
                        var playerChunk = SystemAPI.GetComponentRO<GhostChunk>(playerContainer.ValueRO.player);
                        var to = new SlotPosition(EquipmentConfig.itemInHand_ContainerIndex,0);
                        
                        EQHelper.Clone(ecb,equipmentEvent.slotPosition,to,barsLookup,slotsLookup,containersLookup,playerContainer.ValueRO.player);
                        RPCHelper.SendEventsToClientsAndOwner<NewItemInHandRPC>(ref state,playerNeedChunkLookup,loadedChunks,ecb,ghostOwner.ValueRO.NetworkId,playerChunk.ValueRO.GetChunk(),tick);
                    }
                }
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