using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

[UpdateInGroup(typeof(PresentationSystemGroup))]
[UpdateBefore(typeof(CharacterHandsSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct CharacterHandsEvents : ISystem
{
    private ComponentLookup<LocalTransform> transformLookup;
    private ComponentLookup<AnimationComponent> animationLookup;
    private ComponentLookup<Hands> handsLookup;


    private BufferLookup<AnimationFrames> framesLookup;
    private BufferLookup<AnimationEvents> eventsLookup;

    


    private DynamicBuffer<VisualEffectsBuffer> visualEffects;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkTime>();

        transformLookup = SystemAPI.GetComponentLookup<LocalTransform>();
        animationLookup = SystemAPI.GetComponentLookup<AnimationComponent>();
        handsLookup = SystemAPI.GetComponentLookup<Hands>();
        framesLookup = SystemAPI.GetBufferLookup<AnimationFrames>();
        eventsLookup = SystemAPI.GetBufferLookup<AnimationEvents>();
        state.RequireForUpdate<VisualEffectsBuffer>();


        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<PlayerActionRPC,NewAmmoSelectedRPC>();         
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

    }



    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        
        transformLookup.Update(ref state);
        animationLookup.Update(ref state);
        handsLookup.Update(ref state);
        framesLookup.Update(ref state);
        eventsLookup.Update(ref state);

        state.CompleteDependency();

        var snapshotAck = SystemAPI.GetSingleton<NetworkSnapshotAck>();


        foreach ((RefRO<PlayerActionRPC> action,Entity rpc) in SystemAPI.Query<RefRO<PlayerActionRPC>>().WithEntityAccess())
        {      
            foreach((RefRW<Hands> hands,RefRO<GhostOwner> ghostOwner,RefRO<Velocity2D> vel, Entity e) in SystemAPI.Query<RefRW<Hands>,RefRO<GhostOwner>,RefRO<Velocity2D>>().WithAll<Player>().WithEntityAccess())
            {
                if(ghostOwner.ValueRO.NetworkId != action.ValueRO.networkID) continue;

                if(ItemsAsset.instance.TryGetItem<RangedWeapon>(action.ValueRO.itemID,out var item))
                {
                    StartAnimation(ref state,item,hands.ValueRO,item.shotAnim,animationLookup,transformLookup,framesLookup,eventsLookup); 
                }
                break;
            }
            entityCommandBuffer.DestroyEntity(rpc);
        }

        foreach ((RefRO<NewAmmoSelectedRPC> action,Entity rpc) in SystemAPI.Query<RefRO<NewAmmoSelectedRPC>>().WithEntityAccess())
        {     
            if(snapshotAck.LastReceivedSnapshotByLocal.IsNewerThan(action.ValueRO.tick))
            {             
                foreach((RefRW<Hands> hands,RefRO<GhostOwner> ghostOwner,RefRO<Velocity2D> vel, Entity e) in SystemAPI.Query<RefRW<Hands>,RefRO<GhostOwner>,RefRO<Velocity2D>>().WithAll<Player>().WithEntityAccess())
                {
                    if(ghostOwner.ValueRO.NetworkId != action.ValueRO.networkID) continue;

                    Debug.Log("uwaga new ammo");                       

                    if(ItemsAsset.instance.TryGetItem<RangedWeapon>(action.ValueRO.weaponID,out var item))
                    {
                        StartAnimation(ref state,item,hands.ValueRO,item.reloadAnim,animationLookup,transformLookup,framesLookup,eventsLookup,new int[]{action.ValueRO.ammoID}); 
                    }
                    break;
                }
                entityCommandBuffer.DestroyEntity(rpc);
            }
        }



        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    } 
    public static void ClearAnimationComponent(Entity entity,ComponentLookup<AnimationComponent> animationLookup,ComponentLookup<LocalTransform> transformLookup,BufferLookup<AnimationFrames> framesLookup,BufferLookup<AnimationEvents> eventsLookup)
    {
        var animation = animationLookup.GetRefRW(entity);
        var position = transformLookup.GetRefRW(entity);

        framesLookup[entity].Clear();
        eventsLookup[entity].Clear();
        animation.ValueRW.playbackSpeed = 1f;
        animation.ValueRW.elapsedTime = 0;
        animation.ValueRW.characterCenterPosition = float3.zero;


        if(animation.ValueRO.hasStartPosition)
        {
            position.ValueRW.Position = animation.ValueRO.startPosition;
            position.ValueRW.Rotation = animation.ValueRO.startRotation;
            animation.ValueRW.hasStartPosition = false;
        }
    }
    public static void StartAnimation(ref SystemState state,RangedWeapon item,Hands hands, List<KeyFrame> frames,
    ComponentLookup<AnimationComponent> animationLookup,ComponentLookup<LocalTransform> transformLookup,BufferLookup<AnimationFrames> framesLookup,BufferLookup<AnimationEvents> eventsLookup,int[] args = null)
    {
        ClearAnimationComponent(hands.GetBodyPart(BodyPartType.MainHand),animationLookup,transformLookup,framesLookup,eventsLookup);
        ClearAnimationComponent(hands.GetBodyPart(BodyPartType.SideHand),animationLookup,transformLookup,framesLookup,eventsLookup);
                    
        int index = 0;
        foreach(var frame in frames)
        {
            var part = hands.GetBodyPart(frame.BodyPartType);
            if(frame.BodyPartType == BodyPartType.SideHand && item.twoHanded)
                animationLookup.GetRefRW(part).ValueRW.characterCenterPosition = -1 * transformLookup.GetRefRO(hands.GetBodyPart(BodyPartType.MainHand)).ValueRO.Position + new float3(0,-0.05f,0);
            

            animationLookup.GetRefRW(part).ValueRW.itemID = item.ID;

            animationLookup.GetRefRW(part).ValueRW.itemID = item.ID;
            framesLookup[part].Add(frame.GetAnimationFrame(index));
            foreach (var eventFrame in frame.Events)
            {
                eventsLookup[part].Add(eventFrame.GetEvent(index,args));
            }
            
            state.EntityManager.SetComponentEnabled<AnimationIsPaused>(part,false);
            index++;
        }
    } 
}


