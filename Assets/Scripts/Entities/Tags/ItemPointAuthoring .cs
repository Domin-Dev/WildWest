using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ItemPointAuthoring : MonoBehaviour
{
    public class Baker : Baker<ItemPointAuthoring>
    {
        public override void Bake(ItemPointAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ItemPointTag
            {
            });
        }
    }
}
public struct ItemPointTag : IComponentData
{}

