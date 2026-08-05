using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(TagUpdateSystem))]
public partial struct TagTriggerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<StatefulTriggerEvent,TriggerTagComponent>().WithAll<HasEvents>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
      //  if(!networkTime.IsFirstTimeFullyPredictingTick) return;

        foreach ((DynamicBuffer<StatefulTriggerEvent> triggerEvents,DynamicBuffer<TagActionState> states,RefRO<Parent> parent,RefRO<TriggerTagComponent> tagComponent,EnabledRefRW<HasEvents> hasEvents, Entity trigger) in 
        SystemAPI.Query<DynamicBuffer<StatefulTriggerEvent>,DynamicBuffer<TagActionState>,RefRO<Parent>,RefRO<TriggerTagComponent>,EnabledRefRW<HasEvents>>().WithEntityAccess())
        {   
            if(ItemsAsset.instance.TryGetTag<TagWithTrigger>(tagComponent.ValueRO.TagID,out var tag)
             && ItemsAsset.instance.TryGetItem(tagComponent.ValueRO.itemID,out var item))
            {
                foreach(var tEvent in triggerEvents)   
                {
                    
                    if(tEvent.State != StatefulEventState.Stay)
                        UnityEngine.Debug.Log(" event!! "+ tEvent.GetOtherEntity(trigger) + " " + tEvent.State); 
                    var actions = tag.GetActions(tEvent.State,item);
                    foreach(var action in actions)
                    {
                        action.Run(entityCommandBuffer, new TagWithTriggerEventContext()
                        {
                            entityManager = state.EntityManager,
                            Entity = tEvent.GetOtherEntity(trigger),
                            trigger = trigger,
                            triggerParent = parent.ValueRO.Value,
                            states = states
                        });
                    }
                }
            }
            hasEvents.ValueRW = false;
        }   

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}