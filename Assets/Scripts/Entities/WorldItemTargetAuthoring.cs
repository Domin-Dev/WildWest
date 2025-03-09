using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class WorldItemTargetAuthoring : MonoBehaviour
{
    public Transform target;
    public class Baker : Baker<WorldItemTargetAuthoring>
    {
        public override void Bake(WorldItemTargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new WorldItemTarget
            {
              target = authoring.target,
            }); 
        }
    }

}

public struct WorldItemTarget : IComponentData
{
    public Transform target;
}
