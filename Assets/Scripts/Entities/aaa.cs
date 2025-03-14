using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class aaa : MonoBehaviour
{
    public class Baker : Baker<aaa>
    {
        public override void Bake(aaa authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new aa
            {
              
            });
        }
    }
}

public struct aa : IComponentData
{
    public float3 target;
}

