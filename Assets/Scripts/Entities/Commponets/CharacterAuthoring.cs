using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;



public class CharacterAuthoring : MonoBehaviour
{
    public class Baker : Baker<CharacterAuthoring>
    {
        public override void Bake(CharacterAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Character
            {
                isMove = false
            });
        }
    }
}

public struct Player : IComponentData
{
    public float speed;
}
public struct Character : IComponentData
{
    public bool isMove;
}

