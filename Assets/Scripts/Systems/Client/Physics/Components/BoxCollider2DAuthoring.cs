using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public class BoxCollider2DAuthoring : MonoBehaviour
{
    [SerializeField] float2 size;
    [SerializeField] float2 offset;
    public class Baker : Baker<BoxCollider2DAuthoring>
    {
        public override void Bake(BoxCollider2DAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new BoxCollider2D() { 
                size = authoring.size,
                offset = authoring.offset,
            });
        }
    }
}

public struct BoxCollider2D : IComponentData
{
    public float2 size;
    public float2 offset;
}

