using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

public class HitPointsAuthoring : MonoBehaviour
{
    public int maxHitPoints;
    public class Baker : Baker<HitPointsAuthoring>
    {
        public override void Bake(HitPointsAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new CurrentHitPoints() { value = authoring.maxHitPoints });
            AddComponent(entity, new MaxHitPoints() { value = authoring.maxHitPoints });
            AddBuffer<DamageBufferElement>(entity);
            AddBuffer<DamageThisTick>(entity);
        }
    }
}
