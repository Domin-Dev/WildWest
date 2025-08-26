using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;



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


            AddComponent(entity, new Health());
            AddComponent(entity, new Hunger());
            AddComponent(entity, new Thirst());

            AddBuffer<InventorySlot>(entity);
  
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
    [GhostField(Quantization = 0)] public InputEvent rightButton;
    [GhostField(Quantization = 0)] public InputEvent leftButton;
    [GhostField(Quantization = 0)] public NetworkTick dataTick;

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
    [GhostField] public FixedString128Bytes playerName;
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



[GhostComponent(SendTypeOptimization = GhostSendType.AllClients, OwnerSendType = SendToOwnerType.SendToOwner)]
public struct Health : IComponentData
{
    [GhostField] public int Value;
    [GhostField] public int Max;
}

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients, OwnerSendType = SendToOwnerType.SendToOwner)]
public struct Hunger : IComponentData
{
    [GhostField] public int Value;
    [GhostField] public int Max;
}

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients, OwnerSendType = SendToOwnerType.SendToOwner)]
public struct Thirst : IComponentData
{
    [GhostField] public int Value;
    [GhostField] public int Max;
}

public struct LastChunk : IComponentData
{
    public int value;
}

public struct InterestArea : IComponentData
{
    public float radius;
}
public struct PlayerSourceConnection : IComponentData {
    public Entity value;
}

