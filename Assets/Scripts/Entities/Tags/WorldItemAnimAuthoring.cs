using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class WorldItemAnimAuthoring : MonoBehaviour
{
    public class Baker : Baker<WorldItemAnimAuthoring>
    {
        public override void Bake(WorldItemAnimAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new WorldItemAnim
            { 
            });
        }
    }

}

public struct WorldItemAnim : IComponentData
{
}
