using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using Unity.NetCode.LowLevel;
using Unity.Transforms;

[UpdateInGroup(typeof(TriggerSystemGroup))]
[UpdateBefore(typeof(TagTriggerSystem))]
public partial struct TagTriggerSetUpSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<StatefulTriggerEvent,TriggerTagComponent,NeedSetUp>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((DynamicBuffer<TagActionState> states,RefRO<TriggerTagComponent> tagComponent,EnabledRefRW<NeedSetUp> needSetUp, Entity trigger) in 
        SystemAPI.Query<DynamicBuffer<TagActionState>,RefRO<TriggerTagComponent>,EnabledRefRW<NeedSetUp>>().WithEntityAccess())
        {
            if(ItemsAsset.instance.TryGetTag<TagWithTrigger>(tagComponent.ValueRO.TagID,out var tag)
             && ItemsAsset.instance.TryGetItem(tagComponent.ValueRO.itemID,out var item))
            {
                var actions = tag.onSetUp.GetActions(item);
                foreach(var action in actions)
                    action.Run(entityCommandBuffer,trigger);
                needSetUp.ValueRW = false;
            }
        }   

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}