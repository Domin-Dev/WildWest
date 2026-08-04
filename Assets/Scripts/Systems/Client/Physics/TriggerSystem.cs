using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

public struct EntityPairEvent
{
    public Entity entityA;
    public Entity entityB;
}


[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[UpdateBefore(typeof(AfterPhysicsSystemGroup))]
public partial struct TriggerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<EntitiesReferences>();
    }
    public void OnUpdate(ref SystemState state)
    {   
        var ecbSystem = state.World.GetOrCreateSystemManaged<BeginPredictedSimulationEntityCommandBufferSystem>();
        var ecb = ecbSystem.CreateCommandBuffer();
        bool isClient = state.World.IsClient();

        var job = new TriggerJob()
        {
            destroyEntityLookup = SystemAPI.GetComponentLookup<DestroyEntityTag>(true),
            bulletLookup = SystemAPI.GetComponentLookup<Bullet>(true),
            environmentLookup = SystemAPI.GetComponentLookup<EnvironmentObject>(true),
            positionLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>(),
            hitboxLookup = SystemAPI.GetComponentLookup<HitBoxSettings>(true),
            ghostOwnerLookup = SystemAPI.GetComponentLookup<GhostOwner>(true),
            playerLookup = SystemAPI.GetComponentLookup<Player>(true),
            colliderKeyLookup = SystemAPI.GetBufferLookup<PhysicsColliderKeyEntityPair>(true),
            ecb = ecb,
            isClient = isClient,
            clientNetworkID = isClient ? SystemAPI.GetSingleton<NetworkId>().Value : -1,
        };
        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = job.Schedule(simulationSingleton, state.Dependency);
        ecbSystem.AddJobHandleForProducer(state.Dependency);
    }

}

public struct TriggerJob : ITriggerEventsJob
{

    [ReadOnly] public ComponentLookup<DestroyEntityTag> destroyEntityLookup;
    [ReadOnly] public ComponentLookup<Bullet> bulletLookup;
    [ReadOnly] public ComponentLookup<EnvironmentObject> environmentLookup; 
    [ReadOnly] public ComponentLookup<LocalTransform> positionLookup;
    [ReadOnly] public ComponentLookup<HitBoxSettings> hitboxLookup;
    [ReadOnly] public ComponentLookup<GhostOwner> ghostOwnerLookup;
    [ReadOnly] public ComponentLookup<Player> playerLookup;
    [ReadOnly] public BufferLookup<PhysicsColliderKeyEntityPair> colliderKeyLookup;


  
    public EntityCommandBuffer ecb;
    public EntitiesReferences entitiesReferences;
    public bool isClient;
    public int clientNetworkID;

    public void Execute(TriggerEvent triggerEvent)
    {   
        if(Bullet(triggerEvent)) return;
    }
    public bool Bullet(TriggerEvent triggerEvent)
    {
        Entity bullet; 
        Entity collider;
        ColliderKey colliderKey;

        if(bulletLookup.HasComponent(triggerEvent.EntityA))
        {
            bullet = triggerEvent.EntityA;
            collider = triggerEvent.EntityB;
            colliderKey = triggerEvent.ColliderKeyB;
        }
        else if(bulletLookup.HasComponent(triggerEvent.EntityB))
        {
            bullet = triggerEvent.EntityB;
            collider = triggerEvent.EntityA;
            colliderKey = triggerEvent.ColliderKeyA;
        }
        else
            return false;

        if(destroyEntityLookup.HasComponent(bullet) || destroyEntityLookup.HasComponent(collider)) return true;
        if(BulletHitbox(bullet,collider,colliderKey)) return true;
        if(BulletEnviroment(bullet,collider)) return true;
        return true;
    }
    public bool BulletHitbox(Entity bullet,Entity player, ColliderKey colliderKey)
    {
        if(!playerLookup.HasComponent(player)) return false;

        var buffer = colliderKeyLookup[player];
        Entity hitbox = Entity.Null;
        foreach(var pair in buffer)
        {
            if(pair.Key == colliderKey)
            {
                if(hitboxLookup.HasComponent(pair.Entity))
                {
                    hitbox = pair.Entity;
                    break;
                }
                else
                {
                    return false;
                }
            }
        }
        

        int bulletOwner = ghostOwnerLookup[bullet].NetworkId;
        int hitboxOwner = ghostOwnerLookup[player].NetworkId;


        if(bulletOwner != hitboxOwner)
        {
            var hitBoxSettings = hitboxLookup[hitbox];
            var bulletComponent = bulletLookup[bullet];
            int damage = (int)(bulletComponent.damage * hitBoxSettings.damageMultiplier);

            if(isClient)
            {
                Entity popup = ecb.Instantiate(entitiesReferences.worldTextEntity);
                float3 bulletPos = positionLookup[bullet].Position;
                ecb.SetComponent(popup, LocalTransform.FromPosition(new float3(bulletPos.x,bulletPos.y, -1)));
                ecb.SetComponent(popup, new DamagePopup()
                {
                    lifetime = 1.5f,
                    elapsedTime = 0,
                    moveDirection = new float3(0, 0.4f, 0),
                    damageTag = hitBoxSettings.damageMultiplier > 1f ? DamageTag.Critical : DamageTag.Normal
                });
            }
            else
            {
                ecb.AppendToBuffer<DamageBuffer>(player, new DamageBuffer(){ value = damage});
            }
            ecb.AddComponent(bullet, new DestroyEntityTag());
        }

        return true;
    }
    public bool BulletEnviroment(Entity bullet,Entity enviromentObject)
    {
        if(!environmentLookup.HasComponent(enviromentObject)) return false;

        var transform =  positionLookup[bullet];
        float3 position = transform.Position + transform.Right() * 0.1f;
        if(isClient)
        {
            EntitySpawner.SpawnParticle(ecb,entitiesReferences.spark,new float3(position.x, position.y,0), quaternion.identity);         
        }
        ecb.AddComponent(bullet, new DestroyEntityTag());

        if(isClient)
        {
            Entity popup = ecb.Instantiate(entitiesReferences.worldTextEntity);
            ecb.SetComponent(popup, LocalTransform.FromPosition(new float3(position.x,position.y, -1)));
            ecb.SetComponent(popup, new DamagePopup()
            {
                lifetime = 1.5f,
                elapsedTime = 0,
                moveDirection = new float3(0, 0.4f, 0),
                damageTag = DamageTag.Critical,
                damageValue = 10
            });
        }
        return true;
    }
}