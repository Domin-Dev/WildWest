using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
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
            AddComponent(entity, new Hands());
            AddComponent(entity, new Player() { speed = 1f});
            AddComponent(entity, new NewPlayerTag());

        }
    }
}

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients)]
public struct Player : IComponentData
{
    [GhostField] public float speed;
    [GhostField] public FixedString64Bytes playerName;
}
public struct Character : IComponentData
{
    public bool isMove;

    public float startAnim;

    public int directionHead;
    public int directionBody;

    public Entity body;
    public Entity headParent;
    public Entity head;
}

