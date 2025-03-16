using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class BodyAuthoring : MonoBehaviour
{
    public class Baker : Baker<BodyAuthoring>
    {
        public override void Bake(BodyAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Body
            {

            });
        }
    }
}
public struct Body: IComponentData
{}

