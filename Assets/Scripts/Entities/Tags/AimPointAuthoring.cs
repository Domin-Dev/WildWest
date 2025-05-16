using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AimPointAuthoring : MonoBehaviour
{
    public class Baker : Baker<AimPointAuthoring>
    {
        public override void Bake(AimPointAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new AimPointTag
            {

            });
        }
    }
}
public struct AimPointTag : IComponentData
{}

