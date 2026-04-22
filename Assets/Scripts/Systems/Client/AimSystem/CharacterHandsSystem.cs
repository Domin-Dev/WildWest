using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

[UpdateInGroup(typeof(PresentationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct CharacterHandsSystem : ISystem
{
    private static float leftSide = math.PI / 2f;

    private float deltaTime;

    private ComponentLookup<LocalTransform> transformLookup;
    private ComponentLookup<LocalToWorld> worldLookup;
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
        worldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();
        animationLookup = SystemAPI.GetComponentLookup<AnimationComponent>();
        handsLookup = SystemAPI.GetComponentLookup<Hands>();
        framesLookup = SystemAPI.GetBufferLookup<AnimationFrames>();
        eventsLookup = SystemAPI.GetBufferLookup<AnimationEvents>();
        

        state.RequireForUpdate<VisualEffectsBuffer>();
    }



    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
        deltaTime = SystemAPI.Time.DeltaTime;
        transformLookup.Update(ref state);
        worldLookup.Update(ref state);
        animationLookup.Update(ref state);
        handsLookup.Update(ref state);
        framesLookup.Update(ref state);
        eventsLookup.Update(ref state);

        state.CompleteDependency();
        visualEffects = SystemAPI.GetSingletonBuffer<VisualEffectsBuffer>(true);

        foreach ((RefRW<LocalTransform> position , DynamicBuffer<AnimationFrames> frames, DynamicBuffer<AnimationEvents> animationEvents, RefRW<AnimationComponent> animation, EnabledRefRW<AnimationIsPaused> paused,Entity entity) in 
        SystemAPI.Query<RefRW<LocalTransform>,DynamicBuffer<AnimationFrames>, DynamicBuffer<AnimationEvents>,RefRW<AnimationComponent>,EnabledRefRW<AnimationIsPaused>>().WithDisabled<AnimationIsPaused>().WithEntityAccess())
        {
            if(frames.Length == 0)
            {
                animation.ValueRW.hasStartPosition = false;
                paused.ValueRW = true;
                continue;
            }
            ref var element = ref frames.ElementAt(0);
            animation.ValueRW.elapsedTime += deltaTime * animation.ValueRO.playbackSpeed;

            if(!element.processed)
            {     
                Hands hands = handsLookup[animation.ValueRO.player];
                if(!animation.ValueRO.hasStartPosition)
                {
                    animation.ValueRW.hasStartPosition = true;
                    animation.ValueRW.startRotation = position.ValueRO.Rotation;
                    animation.ValueRW.startPosition = position.ValueRO.Position;
                }       
                element.Process(animation.ValueRO,hands,position.ValueRO);  
            }

            float t = 1;
            if(element.duration > 0)
                t = math.clamp(animation.ValueRO.elapsedTime / element.duration, 0f, 1f);


            element.GetTargetValues(ref state,entity,out var targetPosition,out var targetRotation);

            position.ValueRW.Rotation = math.slerp(position.ValueRO.Rotation, targetRotation, t);
            position.ValueRW.Position = math.lerp(position.ValueRO.Position, targetPosition, t);

            if(t >= 1f)
            {
                Hands hands = handsLookup[animation.ValueRO.player];
                for(int i = animationEvents.Length -1;i >= 0 ; i--)
                {
                    var eventFrame = animationEvents[i];
                    if(eventFrame.frameIndex == element.frameIndex)
                    {
                        switch(eventFrame.eventType)
                        {
                            case EventType.Sound:
                                Sounds.instance.PlayerSound(eventFrame.id);
                                break;
                            case EventType.SpawnParticleAtAimPoint:
                                CreatePrefab(entityCommandBuffer,hands.aimPoint,eventFrame);
                                break;
                            case EventType.SpawnParticleAtReloadPoint:
                                CreatePrefab(entityCommandBuffer,hands.reloadPoint,eventFrame);
                                break;
                            case EventType.ChangeItemSprite:
                                ChangeItemSprite(ref state,hands.itemInMainHand,eventFrame.id,animation.ValueRO.itemID);
                                break;
                            case EventType.ChangeSpriteInSideHand:
                                ChangeSpriteInSideHand(ref state,hands.itemInSideHand,eventFrame);
                                break;
                        }

                        animationEvents.RemoveAtSwapBack(i);
                    }
                }

                position.ValueRW.Rotation = targetRotation;
                position.ValueRW.Position = targetPosition;
                animation.ValueRW.elapsedTime = animation.ValueRO.elapsedTime - element.duration;
                frames.RemoveAt(0);
            }
        }

        foreach ((RefRW<Hands> hands,RefRO<AimRotation> aimRotation, RefRW<Character> character) in SystemAPI.Query<RefRW<Hands>,RefRO<AimRotation>,RefRW<Character>>().WithNone<NewPlayerTag>())
        {
            LocalTransform localSideHand = transformLookup[hands.ValueRO.side];
            LocalToWorld worldMainHand = state.EntityManager.GetComponentData<LocalToWorld>(hands.ValueRO.main);
            LocalTransform localItem =  transformLookup[hands.ValueRO.itemInMainHand];
            LocalTransform localMain =  transformLookup[hands.ValueRO.main];

            
            // if (hands.ValueRO.actionStatus != 0)
            // {
            //     ActionUpdate(hands, ref state);
            // }
            
            float rot = aimRotation.ValueRO.angle;   
            UpdateAimSystem(rot, ref localSideHand, ref localItem, ref localMain, hands);

            transformLookup[hands.ValueRO.main] = localMain;
            transformLookup[hands.ValueRO.side] = localSideHand;
            transformLookup[hands.ValueRO.itemInMainHand] = localItem;
            UpdateDirectionIndex(rot,character, ref state);
        }
        
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }       



    private void ChangeSpriteInSideHand(ref SystemState state,Entity itemInHand,AnimationEvents eventFrame)
    {
        Sprite sprite = null;
        if(eventFrame.id >= 0 && ItemsAsset.instance.TryGetItem(eventFrame.id,out var item))
            sprite = item.GetWorldSprite;

        var pos = state.EntityManager.GetComponentData<LocalTransform>(itemInHand);
        pos.Position = eventFrame.position;
        pos.Rotation = eventFrame.rotation;
        state.EntityManager.SetComponentData(itemInHand,pos);
        state.EntityManager.GetComponentObject<SpriteRenderer>(itemInHand).sprite = sprite;
    }
    private void ChangeItemSprite(ref SystemState state,Entity itemInHand,int spriteID,int itemID)
    {
        if(ItemsAsset.instance.TryGetItem(itemID,out var item))
        {
            if(spriteID < 0)
                state.EntityManager.GetComponentObject<SpriteRenderer>(itemInHand).sprite = item.GetWorldSprite;
            else if(item.animSprites.Count > spriteID)
            {
                state.EntityManager.GetComponentObject<SpriteRenderer>(itemInHand).sprite = item.animSprites[spriteID];    
            } 
        }
    }
    private void CreatePrefab(EntityCommandBuffer entityCommandBuffer, Entity target,AnimationEvents eventFrame)
    {
        LocalToWorld localToWorld = worldLookup[target];
        var prefab = visualEffects[eventFrame.id].entity;


        quaternion rotation;
        if(eventFrame.relativeRotation)
            rotation = math.normalize(math.mul(eventFrame.rotation, localToWorld.Rotation));
        else
            rotation = eventFrame.rotation;

        EntityHelper.SpawnEntityPrefab(entityCommandBuffer,prefab, localToWorld.Position + math.rotate(localToWorld.Rotation, eventFrame.position), rotation ,new NewParticles()
        {
            offset = eventFrame.position,
            target = target,
        });
    }
    
    private void UpdateAimSystem(float angle,ref LocalTransform localSideHand, ref LocalTransform localItem, ref LocalTransform localMain, RefRW<Hands> hands, float maxDeltaTime = 0.05f)
    {
        quaternion mainTargetRotation;
        quaternion sideTargetRotation;
        if (math.abs(angle) > leftSide)
        {
            SetEulerX(180,ref localMain);
            SetEulerX(180,ref localSideHand);

            var p = localItem.Position;
            p.z = -0.0001f;
            localItem.Position = p;
            angle = -angle;

            sideTargetRotation = quaternion.Euler(math.radians(180), 0, angle + math.radians(110));
            mainTargetRotation = quaternion.Euler(math.radians(180), 0, angle);
        }
        else
        {
            SetEulerX(0,ref localMain);
            SetEulerX(0,ref localSideHand);
           
            var p = localItem.Position;
            p.z = 0.0001f;
            localItem.Position = p;
          
            sideTargetRotation = quaternion.Euler(0, 0, angle + math.radians(110));
            mainTargetRotation = quaternion.Euler(0, 0, angle);
        }


          //  localMain.Rotation = mainTargetRotation;
          //  localSideHand.Rotation = sideTargetRotation;
        localMain.Rotation = math.slerp(localMain.Rotation, mainTargetRotation, math.min(deltaTime, maxDeltaTime) * 22);
        localSideHand.Rotation = math.slerp(localSideHand.Rotation, sideTargetRotation, math.min(deltaTime, maxDeltaTime) * 10);

        if (math.Euler(localMain.Rotation).z > 0) 
        {
            localMain.Position.z = 0.0011f;
            localSideHand.Position.z = 0.0011f;
        }
        else
        {
            localMain.Position.z = -0.001f;
            localSideHand.Position.z = -0.001f;
        }
    }
   
    private void SetEulerX(float value, ref LocalTransform localTransform)
    {
        float3 euler = math.Euler(localTransform.Rotation,math.RotationOrder.XYZ);
        euler.x = math.radians(value);
        localTransform.Rotation = quaternion.EulerXYZ(euler);
    }
    public static void GetHandsRotation(float angle,ref LocalTransform localSideHand, ref LocalTransform localItem, ref LocalTransform localMain, RefRW<Hands> hands)
    {
        quaternion mainTargetRotation;
        quaternion sideTargetRotation;

        if (math.abs(angle) > leftSide)
        {
            if (hands.ValueRO.rotated)
            {
                localMain = localMain.RotateX(math.radians(180));
                hands.ValueRW.rotated = false;
                var p = localItem.Position;
                p.z = -0.0001f;
                localItem.Position = p;
            }

            sideTargetRotation = quaternion.Euler(0, 0, angle - math.radians(90));
            angle = -angle;
            mainTargetRotation = quaternion.Euler(math.radians(180), 0, angle);
        }
        else
        {
            if (!hands.ValueRO.rotated)
            {
                localMain = localMain.RotateX(math.radians(-180));
                hands.ValueRW.rotated = true;
                var p = localItem.Position;
                p.z = 0.0001f;
                localItem.Position = p;
            }
            sideTargetRotation = quaternion.Euler(0, 0, angle + math.radians(90));
            mainTargetRotation = quaternion.Euler(0, 0, angle);
        }

        localMain.Rotation =  mainTargetRotation;
        localSideHand.Rotation = sideTargetRotation;

        if (math.Euler(localMain.Rotation).z > 0) localMain.Position.z = 0.0011f;
        else localMain.Position.z = -0.001f;
    }    
    private void UpdateDirectionIndex(float angle, RefRW<Character> character, ref SystemState state)
    {
        int newDirIndex = PlayersInputsServiceClientSystem.GetDirectionIndex(angle);
        if (newDirIndex != character.ValueRO.directionHead)
        {
            character.ValueRW.directionHead = newDirIndex;
            SetDirection(character.ValueRO.head, newDirIndex, ref state);
            if (!character.ValueRO.isMove)
            {
                SetDirection(character.ValueRO.body, newDirIndex, ref state);
                character.ValueRW.directionBody = newDirIndex;
            }
        }
    }
    public static void SetDirection(Entity entity, int newIndex, ref SystemState state)
    {
        if (state.EntityManager.HasComponent<SpriteRenderer>(entity))
        {
            SpriteRenderer spriteRenderer = state.EntityManager.GetComponentObject<SpriteRenderer>(entity);
            MaterialPropertyBlock materialProperty = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(materialProperty);
            materialProperty.SetInt("_Direction", newIndex);
            spriteRenderer.SetPropertyBlock(materialProperty);
        }
    }



    // private void SetActionStatus(ref SystemState state,int index, RefRW<Hands> hands, quaternion lastRot, quaternion targetRot,float3 lastPos, float3 targetPos)
    // {
    //     hands.ValueRW.lastRotation = lastRot;
    //     hands.ValueRW.lastPosition = lastPos;

    //     hands.ValueRW.elapsedTime = 0;

    //     hands.ValueRW.targetPosition = targetPos;
    //     hands.ValueRW.targetRotation = targetRot;

    //     hands.ValueRW.actionStatus = index;
    // }
    // public void ActionUpdate(RefRW<Hands> hands, ref SystemState state)
    // {
    //     LocalTransform localTransform = state.EntityManager.GetComponentData<LocalTransform>(hands.ValueRO.mainhand);
    //     hands.ValueRW.elapsedTime += deltaTime;


    //     float t = math.clamp(hands.ValueRO.elapsedTime / GetActionTime(hands.ValueRO.actionStatus), 0f, 1f);
    //     localTransform.Rotation = math.slerp(localTransform.Rotation, hands.ValueRO.targetRotation, t);
    //     localTransform.Position = math.lerp(localTransform.Position, hands.ValueRO.targetPosition, t);

    //     if(t == 1)
    //     {
    //         if (hands.ValueRO.actionStatus == 1002)
    //         {
    //             localTransform.Position = hands.ValueRO.targetPosition;
    //             localTransform.Rotation = hands.ValueRO.targetRotation;
    //             hands.ValueRW.actionStatus = 0;

    //         }
    //         else
    //         {
    //             hands.ValueRW.targetRotation = hands.ValueRO.lastRotation;
    //             hands.ValueRW.targetPosition = hands.ValueRW.lastPosition;

    //             hands.ValueRW.lastRotation  = localTransform.Rotation;
    //             hands.ValueRW.lastPosition =  localTransform.Position;
    //             hands.ValueRW.elapsedTime = 0;
    //             hands.ValueRW.actionStatus = 1002;
    //         }
    //     }
    //     state.EntityManager.SetComponentData<LocalTransform>(hands.ValueRO.mainhand, localTransform);
    // }
}


