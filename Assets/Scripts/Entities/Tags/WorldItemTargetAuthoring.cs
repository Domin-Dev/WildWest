using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

using Unity.Entities;
using UnityEngine;

public class WorldItemTargetAuthoring : MonoBehaviour
{
    public Transform target;

    public class Baker : Baker<WorldItemTargetAuthoring>
    {
        public override void Bake(WorldItemTargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            Entity targetEntity = GetEntity(authoring.target, TransformUsageFlags.Dynamic);
            AddComponent(entity, new WorldItemTarget
            {
                targetEntity = targetEntity
            });
        }
    }
}
public struct WorldItemTarget : IComponentData
{
    public Entity targetEntity;
}