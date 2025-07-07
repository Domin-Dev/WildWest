using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public struct BoxCollider2D : IComponentData
{
    public float2 size;
    public float2 offset;
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
public struct ForceImpulse2D: IComponentData
{
    public float2 Value;
}

public struct Trigger2D : IComponentData
{
    public float2 Value;
}
public struct IsChanged : IComponentData, IEnableableComponent {  }

public struct AlwaysUpdate: IComponentData, IEnableableComponent { }



public class Physics2DAuthoring : MonoBehaviour
{
    [SerializeField] ushort physicsLayer;
    [SerializeField] bool updateAfterChange;
    public class Baker : Baker<Physics2DAuthoring>
    {
        public override void Bake(Physics2DAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Velocity2D() { Value = float2.zero});
            if (authoring.updateAfterChange)
            {
                AddComponent(entity, new IsChanged());
            }
            else
            {
                AddComponent(entity, new AlwaysUpdate());
            }

            AddComponent(entity, new Physics2D() { 
                layer = authoring.physicsLayer, 
                cellIndex = new int2(int.MinValue, int.MinValue)    
            });
        }
    }

}


