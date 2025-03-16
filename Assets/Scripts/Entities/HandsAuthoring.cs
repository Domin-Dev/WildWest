using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class HandsAuthoring : MonoBehaviour
{
    public class Baker : Baker<CharacterAimAuthoring>
    {
        public override void Bake(CharacterAimAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Hands
            {

            });
        }
    }
}
public struct Hands : IComponentData
{
    public Entity main;
    public Entity side;
}

