using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

using Unity.Entities;
using UnityEngine;

public class WorldItemTargetAuthoring : MonoBehaviour
{
    public Transform target;

    public class Baker : Baker<WorldItemTargetAuthoring>
    {
        public override void Bake(WorldItemTargetAuthoring authoring)
        {
            // Tworzymy encjê w ECS
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            // Tworzymy entity dla targetu (powi¹zanie z GameObject)
            Entity targetEntity = GetEntity(authoring.target, TransformUsageFlags.Dynamic);

            // Dodajemy komponent do encji z odniesieniem do entity targetu
            AddComponent(entity, new WorldItemTarget
            {
                targetEntity = targetEntity
            });
        }


    }
}

public struct WorldItemTarget : IComponentData
{
    public Entity targetEntity;  // Przechowuje Entity, które odpowiada GameObject
}