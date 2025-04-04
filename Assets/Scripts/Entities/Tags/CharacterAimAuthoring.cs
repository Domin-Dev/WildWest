using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class CharacterAimAuthoring : MonoBehaviour
{
    public class Baker : Baker<CharacterAimAuthoring>
    {
        public override void Bake(CharacterAimAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CharacterAim
            {
                target = new float3(1,1,1)
            });
        }
    }
}
public struct CharacterAim : IComponentData
{
    public float3 target;
}

