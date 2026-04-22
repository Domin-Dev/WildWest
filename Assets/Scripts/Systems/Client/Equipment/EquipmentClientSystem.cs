using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst.Intrinsics;
using System.Linq;


public struct EqiupmentEventClient
{
    public EquipmentEventBuffer data;
    public int containerIndex;

    public int slot => data.data.slot;

    public int flag => data.data.flags;

    public EqiupmentEventClient(EquipmentEventBuffer element, int containerIndex)
    {
        this.data = element;
        this.containerIndex = containerIndex;
    }
}




[UpdateInGroup(typeof(EquipmentSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
[BurstCompile]
partial struct EquipmentClientSystem : ISystem
{
    private NativeQueue<EqiupmentEventClient> slotsToUpdate;
    private NetworkTick lastProcessedServerTick;



    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<PlayerContainers> containersLookup;
    private BufferLookup<LinkedContainers> linkedContainers;


    public void OnCreate(ref SystemState state)
    {
        slotsToUpdate = new NativeQueue<EqiupmentEventClient>(Allocator.Persistent);
        lastProcessedServerTick = NetworkTick.Invalid;
        state.RequireForUpdate<EntitiesReferences>();

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>(true);
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>(true);
        containersLookup = SystemAPI.GetBufferLookup<PlayerContainers>(true);
        linkedContainers =  SystemAPI.GetBufferLookup<LinkedContainers>(true);
    }

    public void OnDestroy(ref SystemState state)
    {
        slotsToUpdate.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        var serverTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        if (lastProcessedServerTick.IsValid && serverTick.IsValid &&  !serverTick.IsNewerThan(lastProcessedServerTick))
            return;
        lastProcessedServerTick = serverTick;
        

        slotsToUpdate.Clear();
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        var job = new ProcessEquipmentEventsJob
        {
            SlotsToUpdate = slotsToUpdate.AsParallelWriter(),
            eventBuffer = SystemAPI.GetBufferTypeHandle<EquipmentEventBuffer>(true),
            eventCounter = SystemAPI.GetComponentTypeHandle<EquipmentEventCounter>(),
            container = SystemAPI.GetComponentTypeHandle<ContainerComponent>()
        };
        var query = SystemAPI.QueryBuilder().WithAll<EquipmentEventBuffer, ContainerComponent, EquipmentEventCounter, GhostOwnerIsLocal>().Build();
        JobHandle jobHandle = job.ScheduleParallel(query, state.Dependency);
        jobHandle.Complete();


        if(slotsToUpdate.Count > 0)
        {
            EqiupmentEventClient[] managedArray = new EqiupmentEventClient[slotsToUpdate.Count];
            int index = 0;
            while (slotsToUpdate.Count > 0)
                managedArray[index++] = slotsToUpdate.Dequeue();
            NewEquipmentManager.instance.NewEvents(managedArray,ref entityCommandBuffer);
        }
        
        if(NewEquipmentManager.instance.needUpdateAmmoUI)
        {       
            slotsLookup.Update(ref state);
            barsLookup.Update(ref state);
            containersLookup.Update(ref state);
            linkedContainers.Update(ref state);


            foreach( (var inputSync, var input ,Entity player) in  SystemAPI.Query<RefRW<PlayerInputSync>,RefRW<PlayerInput>>().WithAll<Player,GhostOwnerIsLocal>().WithEntityAccess())
            {
                int ammoID = -1;
                int newAmmoIndex = inputSync.ValueRO.ammoSelectedIndex;
                InventorySlot[] ammo = null;
                InventorySlot[] magazine = null;
                InventorySlot? slot = null;

                if(EQHelper.TryGetPlayerContainer(containersLookup,player,EquipmentConfig.hotBar_ContainerIndex,out var playerContainer))
                {                    
                    EQHelper.TryGetBufferIndex(slotsLookup,inputSync.ValueRO.slotInHand,playerContainer.Value.entity,out slot, out int bufferIndex);   
                    if(slot.HasValue && ItemsAsset.instance.TryGetItem<RangedWeapon>(slot.Value.itemId,out var item))
                    {
                        ammo = EQHelper.TryFindItemWithTag_Aggregated(ref state,slotsLookup,containersLookup,player,item.ammoTagID,out int counter);
                        
                        if(ammo.Length > 0)
                        {
                            if(EQHelper.PlayerHasTheAmmo(inputSync.ValueRO.ammoSelectedItemID,ammo,out int index))
                            {
                                ammoID = inputSync.ValueRO.ammoSelectedItemID;
                                newAmmoIndex = index;
                            }
                            else
                            {
                                newAmmoIndex = newAmmoIndex % ammo.Length;
                                ammoID = ammo[newAmmoIndex].itemId;
                            }
                            Debug.Log("zmaina ammo !" + ammoID);
                        } 

                        Debug.Log("tem " + item.hasMagazine + " " + playerContainer.Value.entity);
                        if(item.hasMagazine)
                            magazine = EQHelper.ReadLinkedContainer(slotsLookup,linkedContainers,playerContainer.Value.entity,slot.Value.slot);

                    }
                }  
                
                Debug.Log("zmaina " + newAmmoIndex);
                inputSync.ValueRW.ammoSelectedItemID = ammoID;
                inputSync.ValueRW.ammoSelectedIndex = newAmmoIndex;
                input.ValueRW.ammoSelectedIndex = newAmmoIndex;
                NewItemInHandSystem.UpdateItemInHandUI(slot,ammo,newAmmoIndex,magazine);  
            }
            NewEquipmentManager.instance.needUpdateAmmoUI = false;
        }

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }


    [BurstCompile]
    struct ProcessEquipmentEventsJob : IJobChunk
    {
        public NativeQueue<EqiupmentEventClient>.ParallelWriter SlotsToUpdate;
        [ReadOnly] public BufferTypeHandle<EquipmentEventBuffer> eventBuffer;
        public ComponentTypeHandle<EquipmentEventCounter> eventCounter;
        [ReadOnly] public ComponentTypeHandle<ContainerComponent> container;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var events = chunk.GetBufferAccessorRO(ref eventBuffer);
            var counters = chunk.GetNativeArray(ref eventCounter);
            var containers = chunk.GetNativeArray(ref container);

            for (int i = 0; i < chunk.Count; i++)
            {
                var eqEvents = events[i];
                if (eqEvents.IsEmpty) continue;
                var counter = counters[i]; 
                int startIndex = 0;
                while (true)
                {
                    bool isEvent = false;
                    for (int j = startIndex; j < eqEvents.Length; j++)
                    {
                        if (eqEvents[j].index == counter.index)
                        {
                            counter.index++;
                            isEvent = true;
                            startIndex = j;
                            SlotsToUpdate.Enqueue(new EqiupmentEventClient(eqEvents[j],containers[i].containerIndex));  
                            break;
                        }
                    }
                    if (!isEvent) break;
                }
                counters[i] = counter;
            } 
        }
    }
}
