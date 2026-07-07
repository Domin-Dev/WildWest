using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;



public class WorldTextAuthoring : MonoBehaviour
{
    public class Baker : Baker<WorldTextAuthoring>
    {
        public override void Bake(WorldTextAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<DamagePopup>(entity, new DamagePopup());
            AddComponent<NewDamagePopup>(entity);
        }
    }
}
public struct DamagePopup : IComponentData
{
    public float3 startPosition;
    public int damageValue;
    public float elapsedTime;
    public float lifetime;
    public float3 moveDirection;
    public DamageTag damageTag;
}


public enum DamageTag : byte
{
    Normal,
    Critical,
    Mining,
}


public struct NewDamagePopup : IComponentData,IEnableableComponent
{
    
}

