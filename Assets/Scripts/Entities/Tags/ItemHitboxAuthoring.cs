using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ItemHitboxAuthoring : MonoBehaviour
{
    public class Baker : Baker<ItemHitboxAuthoring>
    {
        public override void Bake(ItemHitboxAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ItemHitboxTag
            {

            });
        }
    }
}
public struct ItemHitboxTag : IComponentData
{}

