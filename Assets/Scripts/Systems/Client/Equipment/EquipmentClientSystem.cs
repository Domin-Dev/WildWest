using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst.Intrinsics;
using System.Linq;
using Unity.Entities.UniversalDelegates;


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

    public void OnCreate(ref SystemState state)
    {
        slotsToUpdate = new NativeQueue<EqiupmentEventClient>(Allocator.Persistent);
        lastProcessedServerTick = NetworkTick.Invalid;
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
