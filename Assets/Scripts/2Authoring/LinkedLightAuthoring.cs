using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class LinkedLightAuthoring : MonoBehaviour
{
    [SerializeField] private float Intensity;
    [SerializeField] private float OuterRadius;
    [SerializeField] private float FalloffIntensity;
    [SerializeField] private float InnerRadius;
    [SerializeField] private float3 Offset;
    public class Baker : Baker<LinkedLightAuthoring>
    {
        public override void Bake(LinkedLightAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity,new LinkedLight()
            {
                FalloffIntensity = authoring.FalloffIntensity,
                InnerRadius = authoring.InnerRadius,
                Intensity = authoring.Intensity,
                OuterRadius = authoring.OuterRadius,
                Offset = authoring.Offset
            });
        }
    }
}

