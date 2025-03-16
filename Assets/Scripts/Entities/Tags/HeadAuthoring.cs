using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class HeadAuthoring : MonoBehaviour
{
    public class Baker : Baker<HeadAuthoring>
    {
        public override void Bake(HeadAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Head
            {

            });
        }
    }
}
public struct Head: IComponentData
{}

