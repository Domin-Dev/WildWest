using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(DestroySystemGroup))]
public partial class DestroyWorldItemsSystem : SystemBase
{
    protected override void OnCreate()
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
       .WithAll<DestroyEntityTag,WorldItem>();
        RequireForUpdate(GetEntityQuery(entityQueryBuilder));
    }

    protected override void OnUpdate()
    {
        foreach (var (localTransform, worldItem, entity) in SystemAPI.Query<RefRW<LocalTransform>,RefRO<WorldItem>>().WithAll<DestroyEntityTag>().WithEntityAccess())
        {
            foreach((RefRO<ChunkComponent> chunkComponent, DynamicBuffer<WorldItems> worldItems) in SystemAPI.Query<RefRO<ChunkComponent>,DynamicBuffer<WorldItems>>())
            {
                if(chunkComponent.ValueRO.chunkIndex != worldItem.ValueRO.chunkIndex) continue;
                for(int i = 0; i < worldItems.Length; i++)
                {
                    if(worldItems[i].slot == worldItem.ValueRO.slotIndex)
                    {
                        worldItems.RemoveAtSwapBack(i);
                        break;
                    }
                }
                break;
            }
        }
    }
}

