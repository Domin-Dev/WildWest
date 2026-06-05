

using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;



public class WorldItemAuthoring : MonoBehaviour
{
    public class Baker : Baker<WorldItemAuthoring>
    {
        public override void Bake(WorldItemAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<MoveToTarget>(entity);
        }
    }
}
public struct MoveToTarget : IComponentData
{
    public NetworkTick startTick;
    public float2 target;
    public float2 startPosition;
    public float duration; 
}

