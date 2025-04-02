using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public struct Hitbox2D : IComponentData
{
    public float2 size;
}
public struct Physics2D : IComponentData
{
    public int2 cellIndex;
    public ushort layer;
}
public struct Velocity2D : IComponentData
{
    public float2 Value;
}
public struct Trigger2D : IComponentData
{
    public float2 Value;
}
public struct IsChanged : IComponentData, IEnableableComponent { } 




public class Physics2DAuthoring : MonoBehaviour
{
    [SerializeField] float2 hitboxSize; 
    [SerializeField] ushort physicsLayer;
    public class Baker : Baker<Physics2DAuthoring>
    {
        public override void Bake(Physics2DAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Velocity2D() { Value = float2.zero});
            AddComponent(entity, new IsChanged());
            AddComponent(entity, new Physics2D() { 
                layer = authoring.physicsLayer, 
                cellIndex = new int2(int.MinValue, int.MinValue)    
            });
            AddComponent(entity, new Hitbox2D() {
                size = authoring.hitboxSize,
            }); 
        }
    }

}


