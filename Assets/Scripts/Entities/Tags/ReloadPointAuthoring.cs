using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ReloadPointAuthoring : MonoBehaviour
{
    public class Baker : Baker<ReloadPointAuthoring>
    {
        public override void Bake(ReloadPointAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ReloadPointTag
            {

            });
        }
    }
}
public struct ReloadPointTag : IComponentData
{}

