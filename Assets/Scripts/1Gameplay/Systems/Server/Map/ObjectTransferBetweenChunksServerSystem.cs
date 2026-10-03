

// using Unity.Burst;
// using Unity.Burst.Intrinsics;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.NetCode;

// [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
// partial struct ObjectTransferBetweenChunksServerSystem : ISystem
// {
//     

//     [BurstCompile]
//     struct ProcessEquipmentEventsJob : IJobChunk
//     {
//         public NativeQueue<EqiupmentEventClient>.ParallelWriter SlotsToUpdate;
//         [ReadOnly] public BufferTypeHandle<EquipmentEventBuffer> eventBuffer;
//         public ComponentTypeHandle<EquipmentEventCounter> eventCounter;
//         [ReadOnly] public ComponentTypeHandle<ContainerComponent> container;

//         public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
//         {
//             var events = chunk.GetBufferAccessorRO(ref eventBuffer);
//             var counters = chunk.GetNativeArray(ref eventCounter);
//             var containers = chunk.GetNativeArray(ref container);

//             for (int i = 0; i < chunk.Count; i++)
//             {
//                 var eqEvents = events[i];
//                 if (eqEvents.IsEmpty) continue;
//                 var counter = counters[i]; 
//                 int startIndex = 0;
//                 while (true)
//                 {
//                     bool isEvent = false;
//                     for (int j = startIndex; j < eqEvents.Length; j++)
//                     {
//                         if (eqEvents[j].index == counter.index)
//                         {
//                             counter.index++;
//                             isEvent = true;
//                             startIndex = j;
//                             SlotsToUpdate.Enqueue(new  EqiupmentEventClient(eqEvents[j],containers[i].containerIndex));  
//                             break;
//                         }
//                     }
//                     if (!isEvent) break;
//                 }
//                 counters[i] = counter;
//             } 
//         }
//     }
// }
