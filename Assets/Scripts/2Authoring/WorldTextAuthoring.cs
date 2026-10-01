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




