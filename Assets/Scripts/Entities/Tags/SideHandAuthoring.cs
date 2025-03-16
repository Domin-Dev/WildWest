using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SideHandAuthoring : MonoBehaviour
{
    public class Baker : Baker<SideHandAuthoring>
    {
        public override void Bake(SideHandAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SideHand
            {

            });
        }
    }
}
public struct SideHand : IComponentData
{}

