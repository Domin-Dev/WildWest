using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
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
            AddComponent(entity, new PlayerInput()
            {
                sightDirection = new float2(float.MinValue,float.MinValue),
                slotInHand = 0
            });
            AddComponent(entity, new PlayerInputSync()
            {
                slotInHand = -1
            });
            AddComponent(entity, new PlayerLook());

            AddComponent(entity, new PlayerActionSpread());
            AddComponent(entity, new ShootingSkill());




            AddComponent(entity, new AimRotation());
            AddComponent(entity, new Cooldown());
            AddComponent(entity, new CurrentPlayerState(){ state = PlayerState.none});
            
            AddComponent(entity, new GhostChunk().StartValues());

 
            AddComponent(entity, new Health());
            AddComponent(entity, new Hunger());
            AddComponent(entity, new Thirst());


            AddBuffer<DamageBuffer>(entity);
            AddBuffer<EntityContainers>(entity);
        }
    }
}


[GhostComponent(SendTypeOptimization = GhostSendType.AllClients,OwnerSendType = SendToOwnerType.SendToOwner)]
public struct ShootingSkill : IComponentData
{
    public byte Value;
}




[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerInput : IInputComponentData
{
    [GhostField(Quantization = 0)] public float2 movementDirection;
    [GhostField(Quantization = 0)] public float2 sightDirection;
    [GhostField(Quantization = 0)] public InputEvent rightButton;
    [GhostField(Quantization = 0)] public InputEvent leftButton;
    [GhostField(Quantization = 0)] public InputEvent reloadButton;
    [GhostField(Quantization = 0)] public InputEvent unloadButton;


    [GhostField(Quantization = 0)] public int slotInHand;
    [GhostField(Quantization = 0)] public int ammoSelectedIndex;

    [GhostField(Quantization = 0)] public NetworkTick dataTick;
    
    public bool SightDirectionIsEmpty()
    {
        return sightDirection.x == float.MinValue && sightDirection.y == float.MinValue;
    }
}

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients,OwnerSendType = SendToOwnerType.SendToNonOwner)]
public struct PlayerInputSync : IComponentData
{
    [GhostField] public float2 movementDir;
    [GhostField] public float2 sightPosition;
    
    [GhostField] public InputEvent rightButton;
    [GhostField] public InputEvent leftButton;



    public int slotInHand; 
    public int ammoSelectedIndex;
    public int ammoSelectedItemID;
    public int ammoSelectedTagID;
}



[GhostComponent(SendTypeOptimization = GhostSendType.AllClients)]
public struct PlayerLook : IComponentData
{
    [GhostField] public CharacterLook look;
}


public struct ContainersLoaded : IComponentData{}

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients)]
public struct Player : IComponentData
{
    [GhostField] public float speed;
    [GhostField] public FixedString128Bytes playerName;

    [GhostField] public float armor; 
    [GhostField] public float insulation;
    [GhostField] public float waterResistance;
    [GhostField] public float aesthetic;
    [GhostField] public float wetness;
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

    public Entity GetPart(BodyPart part)
    {
        switch(part)
        {
            case BodyPart.head:
                return head;
            case BodyPart.body:
                return body;
        }
        return Entity.Null;
    }

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


public struct GhostChunk : IComponentData
{
    public int spawnChunk;
    public int current;
    public int lastChunk;
    
    public Entity currentChunkEntity;
    public float3 lastPosition;

    public readonly static float3 incorrectPosition = new float3(float.MinValue,float.MinValue,float.MinValue);

    public GhostChunk StartValues()
    {
        current = int.MinValue;
        lastChunk = int.MinValue;
        spawnChunk = int.MinValue;
        lastPosition = incorrectPosition;
        return this;
    }

    public GhostChunk(GhostChunk ghostChunk)
    {
        this.current = ghostChunk.current;
        this.spawnChunk = ghostChunk.spawnChunk;
        this.lastChunk = ghostChunk.lastChunk;
        this.currentChunkEntity = ghostChunk.currentChunkEntity;
        this.lastPosition = ghostChunk.lastPosition;
    }
    
    public bool LastPositionIsCorrect()
    {
        return lastPosition.x != incorrectPosition.x;
    }

    public void SetChunkEntity(Entity entity)
    {
        this.currentChunkEntity = entity;
    }
    public int GetLastChunk()
    {
        if(LastChunkIsNull() && !SpawnChunkIsNull())
        {
            int spawn = spawnChunk;
            spawnChunk = int.MinValue;
            return spawn;
        }
        return lastChunk;
    }
    public int GetChunk()
    {
        if(CurrentChunkIsNull())
            return spawnChunk;
        else
            return current;
    }

    public bool HasChunk()
    {
        return GetChunk() != int.MinValue;
    }

    public bool SpawnChunkIsNull()
    {
        return current == int.MinValue;
    }
    public bool CurrentChunkIsNull()
    {
        return current == int.MinValue;
    }
    public bool LastChunkIsNull()
    {
        return lastChunk == int.MinValue;
    }
    public void SetNewChunk(int newChunk = int.MinValue)
    {
        lastChunk = current;
        current = newChunk;
    }
}



public struct PlayerSourceConnection : IComponentData {
    public Entity value;
}




// Player Actions

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients, OwnerSendType = SendToOwnerType.SendToNonOwner)]
public struct AimRotation : IComponentData
{
    [GhostField] public float angle;
} 


public struct PlayerActionSpread : IComponentData
{
    public float Spread;
} 




public struct Cooldown : IComponentData
{
    public NetworkTick cooldownTick;
    public NetworkTick startCooldown;
}
public struct CurrentPlayerState : IComponentData
{
    public PlayerState state;
}


public enum PlayerState : byte
{
    none,
    reloading,
    reloadingNoMagazine,
    shooting,
    unloading
}




public struct PlayerChunks : IBufferElementData
{
    public int chunkIndex;
    public Entity chunkEntity;
    public double time;
}