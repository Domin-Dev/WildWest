using System;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine.Diagnostics;
using System.Linq;


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
    private ComponentLookup<PlayerInputSync> playerInputSyncLookup;


    private NetworkTick tick;
    private DynamicBuffer<LoadedChunks> loadedChunks;
    private NativeHashSet<Entity> playersToAmmoUpdate;



    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EquipmentEvent>();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>();
        containersLookup = SystemAPI.GetBufferLookup<PlayerContainers>();
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>();
        playerNeedChunkLookup = SystemAPI.GetBufferLookup<PlayersNeedChunk>();
        playerInputSyncLookup = SystemAPI.GetComponentLookup<PlayerInputSync>();
        playersToAmmoUpdate = new NativeHashSet<Entity>(16,Allocator.Persistent);
    }

    public void OnDestroy(ref SystemState state)
    {
        playersToAmmoUpdate.Dispose();
    }


    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        slotsLookup.Update(ref state);
        containersLookup.Update(ref state);
        barsLookup.Update(ref state);
        playerNeedChunkLookup.Update(ref state);
        playerInputSyncLookup.Update(ref state);


        loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(true);
        tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        playersToAmmoUpdate.Clear();

        foreach ((RefRO<EquipmentEvent> eventData,Entity entity)
        in SystemAPI.Query<RefRO<EquipmentEvent>>().WithEntityAccess())
        {
            var playerToAmmoUpdate = CreateNewEquipmentEvent(ref state,ecb,eventData.ValueRO);
            if(playerToAmmoUpdate != Entity.Null && !playersToAmmoUpdate.Contains(playerToAmmoUpdate))
                playersToAmmoUpdate.Add(playerToAmmoUpdate);
            ecb.DestroyEntity(entity);
        }


        Debug.Log("nowy  lista " + playersToAmmoUpdate.Count);
        foreach(var player in playersToAmmoUpdate)
        {
            if(EQHelper.TryGetBufferIndex(slotsLookup,containersLookup,player,EquipmentConfig.itemInHand_SlotPosition, out var slot,out int bufferIndex) && 
            ItemsAsset.instance.TryGetItem<RangedWeapon>(slot.Value.itemId,out var item) && !item.hasMagazine)
            {
                var ammo = EQHelper.TryFindItemWithTag_Aggregated(ref state,slotsLookup,containersLookup,player,item.ammoTagID,out int counter);
                var input = playerInputSyncLookup[player];

                if((input.ammoSelectedItemID < 0 && ammo.Length > 0) || (input.ammoSelectedItemID >= 0 && ammo.Length == 0))
                {                 
                    var ghostChunk = state.EntityManager.GetComponentData<GhostChunk>(player);   
                    var ghostOwner = state.EntityManager.GetComponentData<GhostOwner>(player);   
                    var ammoID = -1; 

                    if(ammo.Length > 0)
                    {
                        if(EQHelper.PlayerHasTheAmmo(input.ammoSelectedItemID,ammo))
                            ammoID = input.ammoSelectedItemID;
                        else
                        {
                            var selectedAmmo = input.ammoSelectedIndex % ammo.Length;
                            ammoID = ammo[selectedAmmo].itemId;
                            playerInputSyncLookup.GetRefRW(player).ValueRW.ammoSelectedItemID = ammoID;
                        }
                    }
                    else
                        playerInputSyncLookup.GetRefRW(player).ValueRW.ammoSelectedItemID = ammoID;

                    RPCHelper.SendEventsToClientsAndOwner<NewAmmoSelectedRPC>(new NewAmmoSelectedRPC(){ ammoID = ammoID ,weaponID =  slot.Value.itemId} ,
                    ref state,playerNeedChunkLookup,loadedChunks,ecb,ghostOwner.NetworkId,player,ghostChunk.GetChunk(),tick,true);                    
                }

            }
        }




        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    public Entity CreateNewEquipmentEvent(ref SystemState state,EntityCommandBuffer ecb,EquipmentEvent equipmentEvent)
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
                
                var input = playerInputSyncLookup.GetRefRW(playerContainer.ValueRO.player);         
                if(equipmentEvent.containerIndex == EquipmentConfig.hotBar_ContainerIndex || equipmentEvent.data.flags == EquipementEventFlags.ClearAllContainers)
                {
                    if(input.ValueRO.slotInHand == equipmentEvent.slotPosition.slotIndex)
                    {
                        var playerChunk = SystemAPI.GetComponentRO<GhostChunk>(playerContainer.ValueRO.player);
                        var to = new SlotPosition(EquipmentConfig.itemInHand_ContainerIndex,0);
                        
                        EQHelper.Clone(equipmentEvent.slotPosition,to,barsLookup,slotsLookup,containersLookup,playerContainer.ValueRO.player,out var newSlot,out var newBarData);
                        int itemID = newSlot.HasValue ? newSlot.Value.itemId : -1;
                        RPCHelper.SendEventsToClientsAndOwner<NewItemInHandRPC>(new NewItemInHandRPC(){ itemID = itemID},ref state,playerNeedChunkLookup,loadedChunks,ecb,ghostOwner.ValueRO.NetworkId,playerContainer.ValueRO.player,playerChunk.ValueRO.GetChunk(),tick);
                    }
                }

                return playerContainer.ValueRO.player;  
            }
        }
        return Entity.Null;
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