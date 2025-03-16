using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class MainHandAuthoring : MonoBehaviour
{
    public class Baker : Baker<MainHandAuthoring>
    {
        public override void Bake(MainHandAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new MainHand
            {

            });
        }
    }
}
public struct MainHand : IComponentData
{}

