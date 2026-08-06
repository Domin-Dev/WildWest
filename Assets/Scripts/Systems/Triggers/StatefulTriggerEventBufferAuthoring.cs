using UnityEngine;
using Unity.Entities;
using Unity.Physics;

public class StatefulTriggerEventBufferAuthoring : MonoBehaviour
{
    class Baker : Baker<StatefulTriggerEventBufferAuthoring>
    {
        public override void Bake(StatefulTriggerEventBufferAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddBuffer<StatefulTriggerEvent>(entity);
            AddComponent<HasEvents>(entity);
        }
    }
}
public struct StatefulTriggerEvent : IStatefulSimulationEvent<StatefulTriggerEvent>
{
    public Entity EntityA { get; set; }
    public Entity EntityB { get; set; }
    public int BodyIndexA { get; set; }
    public int BodyIndexB { get; set; }
    public ColliderKey ColliderKeyA { get; set; }
    public ColliderKey ColliderKeyB { get; set; }
    public StatefulEventState State { get; set; }

    public StatefulTriggerEvent(TriggerEvent triggerEvent)
    {
        EntityA = triggerEvent.EntityA;
        EntityB = triggerEvent.EntityB;
        BodyIndexA = triggerEvent.BodyIndexA;
        BodyIndexB = triggerEvent.BodyIndexB;
        ColliderKeyA = triggerEvent.ColliderKeyA;
        ColliderKeyB = triggerEvent.ColliderKeyB;
        State = default;
    }
    public Entity GetOtherEntity(Entity entity)
    {
        return (entity == EntityA) ? EntityB : EntityA;
    }

    public int CompareTo(StatefulTriggerEvent other) => ISimulationEventUtilities.CompareEvents(this, other);
}
public struct StatefulTriggerEventExclude : IComponentData {}
public struct HasEvents : IComponentData, IEnableableComponent {}
public struct NeedSetUp : IComponentData, IEnableableComponent {}