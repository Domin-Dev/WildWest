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

            AddComponent(entity, new Hands());
            AddComponent(entity, new Character());
            AddComponent(entity, new Player() { speed = 1f });
            AddComponent(entity, new NewPlayerTag());
            AddComponent(entity, new PlayerInput());
            AddComponent(entity, new PlayerInputSync());
            AddComponent(entity, new ItemInHandInput() { itemInHand = int.MinValue });
            AddComponent(entity, new ItemInHandInputSync() { itemInHand = int.MinValue});
            AddComponent(entity, new PlayerLook());

            AddBuffer<CooldownTargetTick>(entity);
        }
    }
}


[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerInput : IInputComponentData
{
    [GhostField(Quantization = 0)] public float2 movementDirection;
    [GhostField(Quantization = 0)] public float2 sightDirection;
    [GhostField(Quantization = 0)] public quaternion handRotation;
    public InputEvent rightButton;
    public InputEvent leftButton;
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct ItemInHandInput : IInputComponentData
{
    [GhostField(Quantization = 0)] public int itemInHand;
}


[GhostComponent(SendTypeOptimization = GhostSendType.AllClients,OwnerSendType = SendToOwnerType.SendToNonOwner)]
public struct PlayerInputSync : IComponentData
{
    [GhostField] public float2 movementDir;
    [GhostField] public float2 sightDirection;
    [GhostField] public quaternion handRotation;


    [GhostField] public InputEvent rightButton;
    [GhostField] public InputEvent leftButton;
}

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients, OwnerSendType = SendToOwnerType.SendToNonOwner)]
public struct ItemInHandInputSync : IComponentData
{
    [GhostField] public int itemInHand;
}


[GhostComponent(SendTypeOptimization = GhostSendType.AllClients)]
public struct PlayerLook : IComponentData
{
    [GhostField] public CharacterLook look;
}


[GhostComponent(SendTypeOptimization = GhostSendType.AllClients)]
public struct Player : IComponentData
{
    public float speed;
    [GhostField] public FixedString64Bytes playerName;
    [GhostField] public bool isCooldown;
    [GhostField] public NetworkTick cooldownTick;
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

