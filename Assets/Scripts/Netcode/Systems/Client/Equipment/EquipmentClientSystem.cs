using Game.Client.Map;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct EquipmentClientSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {   
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((DynamicBuffer<EquipmentEventBuffer> events, RefRO<ContainerComponent> container, RefRW<EquipmentEventCounter> counter) in 
        SystemAPI.Query<DynamicBuffer<EquipmentEventBuffer>,RefRO<ContainerComponent>,RefRW<EquipmentEventCounter>>())
        {
            if (events.IsEmpty) continue;
            while (true)
            {
                bool isEvent = false;
                for (int i = 0; i < events.Length; i++)
                {
                    var ev = events[i];
                    if (ev.index == counter.ValueRO.index)
                    {
                        counter.ValueRW.index++;

                        switch (ev.data.flags)
                        {
                            case 1:
                                Debug.Log(" aaaaaAaaaaaaaaaa" + ev.data.slot); 
                                if (ev.data.slot >= 0)                          
                                    NewEquipmentManager.instance.UpdateSlotIndex(new SlotPosition(container.ValueRO.containerIndex, ev.data.slot));
                                break;
                            case 2:
                                    NewEquipmentManager.instance.ClearContainer(container.ValueRO.containerIndex,ref entityCommandBuffer);
                                break;
                            case 3:
                                    NewEquipmentManager.instance.ClearAllContainers(ref entityCommandBuffer);
                                break;
                            case 4:
                                    NewEquipmentManager.instance.UpdateWetness();
                                break;
                        }
                        isEvent = true;
                        break;
                    }
                }
                if (!isEvent) break;
            }
        }
        
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
