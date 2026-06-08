using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

public struct HitBoxSettings : IComponentData
{
    public float damageMultiplier;
}

public class HitBoxSettingsAuthoring : MonoBehaviour
{
    [SerializeField] float damageMultiplier;
    public class Baker : Baker<HitBoxSettingsAuthoring>
    {
        public override void Bake(HitBoxSettingsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new HitBoxSettings() { 
                damageMultiplier = authoring.damageMultiplier
            });
        }
    }

}


