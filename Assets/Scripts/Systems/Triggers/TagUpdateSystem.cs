using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;




[UpdateInGroup(typeof(TriggerSystemGroup))]
public partial struct TagUpdateSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<TriggerTagComponent>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach ((RefRO<TriggerTagComponent> tagComponent,DynamicBuffer<TagActionState> states,RefRO<TagWithTriggerEventContext> tagUpdate, Entity trigger)
         in SystemAPI.Query<RefRO<TriggerTagComponent>,DynamicBuffer<TagActionState>,RefRO<TagWithTriggerEventContext>>().WithAll<Simulate>().WithEntityAccess())
        {
            var context = tagUpdate.ValueRO;
            context.states = states;

            if(ItemsAsset.instance.TryGetTag<TagWithTrigger>(tagComponent.ValueRO.TagID,out var tag)
             && ItemsAsset.instance.TryGetItem(tagComponent.ValueRO.itemID,out var item))
            {
                var actions = tag.onUpdate.GetActions(item);
                foreach(var action in actions)
                {
                    action.Run(entityCommandBuffer,context,deltaTime);
                }
            }
        }   

        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

}


