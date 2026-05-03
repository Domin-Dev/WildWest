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
using UnityEngine.XR;

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

    
    private BufferLookup<InventorySlot> slotsLookup;
    private BufferLookup<ItemBarData> barsLookup;
    private BufferLookup<PlayerContainers> containersLookup;


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

        slotsLookup = SystemAPI.GetBufferLookup<InventorySlot>(true);
        barsLookup = SystemAPI.GetBufferLookup<ItemBarData>(true);
        containersLookup = SystemAPI.GetBufferLookup<PlayerContainers>(true);

        state.RequireForUpdate<VisualEffectsBuffer>();

        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAny<PlayerActionRPC,NewAmmoSelectedRPC,EmptyMagazineRPC,StopReloadRPC,UnloadRPC>();   

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

        slotsLookup.Update(ref state);
        barsLookup.Update(ref state);
        containersLookup.Update(ref state);

        state.CompleteDependency();
        var currentTime = SystemAPI.GetSingleton<NetworkTime>();
        


        foreach ((RefRO<PlayerActionRPC> action,Entity rpc) in SystemAPI.Query<RefRO<PlayerActionRPC>>().WithEntityAccess())
        {      
            foreach((RefRW<Hands> hands,RefRO<GhostOwner> ghostOwner,RefRO<Velocity2D> vel, Entity e) in SystemAPI.Query<RefRW<Hands>,RefRO<GhostOwner>,RefRO<Velocity2D>>().WithAll<Player,ContainersLoaded>().WithEntityAccess())
            {
                if(ghostOwner.ValueRO.NetworkId != action.ValueRO.networkID) continue;
                if(ItemsAsset.instance.TryGetItem<RangedWeapon>(action.ValueRO.itemID,out var item))
                {
                    state.EntityManager.SetComponentData<CurrentPlayerState>(e,new CurrentPlayerState(){ state = PlayerState.shooting});
                    StartAnimation(ref state,item,hands,item.shotAnim,animationLookup,transformLookup,framesLookup,eventsLookup); 
                }
                break;
            }
            entityCommandBuffer.DestroyEntity(rpc);
        }  
        
        foreach ((RefRO<EmptyMagazineRPC> action,Entity rpc) in SystemAPI.Query<RefRO<EmptyMagazineRPC>>().WithEntityAccess())
        {      
            Debug.Log("jest!!");
            foreach((RefRW<Hands> hands,RefRO<GhostOwner> ghostOwner,RefRO<Velocity2D> vel, Entity e) in SystemAPI.Query<RefRW<Hands>,RefRO<GhostOwner>,RefRO<Velocity2D>>().WithAll<Player,ContainersLoaded>().WithEntityAccess())
            {
                if(ghostOwner.ValueRO.NetworkId != action.ValueRO.networkID) continue;
                if(ItemsAsset.instance.TryGetItem<RangedWeapon>(action.ValueRO.itemID,out var item))
                {
                    float time = EntityHelper.TicksToSeconds(currentTime.ServerTick.TicksSince(action.ValueRO.tick));
                    StartAnimation(ref state,item,hands,item.emptyMagazine,animationLookup,transformLookup,framesLookup,eventsLookup,time); 
                }
                break;
            }
            entityCommandBuffer.DestroyEntity(rpc);
        }

        foreach ((RefRO<NewAmmoSelectedRPC> action,Entity rpc) in SystemAPI.Query<RefRO<NewAmmoSelectedRPC>>().WithEntityAccess())
        {   
         //   Debug.Log("bbbbbbbbbbbbbbbbbbbbbbbbb " + currentTime.InterpolationTick.TickIndexForValidTick + "  " + action.ValueRO.tick.TickIndexForValidTick);
            if(currentTime.InterpolationTick.IsNewerThan(action.ValueRO.tick))
            {            
                foreach((RefRW<Hands> hands,RefRO<GhostOwner> ghostOwner,RefRO<PlayerInputSync> input, Entity player) in SystemAPI.Query<RefRW<Hands>,RefRO<GhostOwner>,RefRO<PlayerInputSync>>().WithAll<Player,ContainersLoaded>().WithEntityAccess())
                {
                    if(ghostOwner.ValueRO.NetworkId != action.ValueRO.networkID) continue;
                    if(SystemAPI.HasComponent<GhostOwnerIsLocal>(player))
                    {
                        if(EQHelper.TryGetPlayerContainer(containersLookup,player,EquipmentConfig.hotBar_ContainerIndex,out var playerContainer))
                        {
                            EQHelper.TryGetBufferIndex(slotsLookup,input.ValueRO.slotInHand,playerContainer.Value.entity,out InventorySlot? slot , out int bufferIndex);
                            int itemId = slot.HasValue ? slot.Value.itemId : -1;
                            if(itemId != action.ValueRO.weaponID) break;
                        }
                    }
                                    
                    if(ItemsAsset.instance.TryGetItem<RangedWeapon>(action.ValueRO.weaponID,out var item))
                    {
                        float time = EntityHelper.TicksToSeconds(currentTime.ServerTick.TicksSince(action.ValueRO.tick));

                        if(action.ValueRO.ammoID >= 0)
                        {
                            state.EntityManager.SetComponentData<Cooldown>(player,new Cooldown(){ cooldownTick = EntityHelper.AddTime(action.ValueRO.tick,item.reloadCooldown)});
                            state.EntityManager.SetComponentData<CurrentPlayerState>(player,new CurrentPlayerState(){ state = item.reloadingState});
                            StartAnimation(ref state,item,hands,item.reloadAnim,animationLookup,transformLookup,framesLookup,eventsLookup,time,new int[]{action.ValueRO.ammoID}); 
                        }
                        else
                        {
                            StartAnimation(ref state,item,hands,item.lostAmmo,animationLookup,transformLookup,framesLookup,eventsLookup,time,null); 
                        }
                    }
                    entityCommandBuffer.DestroyEntity(rpc);
                    break;
                }
            }
            else if(SystemAPI.HasComponent<ReceiveRpcCommandRequest>(rpc))
            {
               var command = SystemAPI.GetComponent<ReceiveRpcCommandRequest>(rpc);
                if(!command.IsConsumed)
                {
                    command.Consume();
                    SystemAPI.SetComponent(rpc,command);
                }
            }
                
        }

        foreach ((RefRO<StopReloadRPC> action,Entity rpc) in SystemAPI.Query<RefRO<StopReloadRPC>>().WithEntityAccess())
        {    
                foreach((RefRW<Hands> hands,RefRO<GhostOwner> ghostOwner,RefRO<Velocity2D> vel, Entity e) in SystemAPI.Query<RefRW<Hands>,RefRO<GhostOwner>,RefRO<Velocity2D>>().WithAll<Player,ContainersLoaded>().WithEntityAccess())
                {
                    if(ghostOwner.ValueRO.NetworkId != action.ValueRO.networkID) continue;
                    state.EntityManager.SetComponentData<Cooldown>(e,new Cooldown(){ cooldownTick = EntityHelper.AddTime(action.ValueRO.tick,20) });
                    var item = ItemsAsset.instance.GetItem(action.ValueRO.weaponID);
                    ResetAnimation(hands.ValueRO,animationLookup,transformLookup,framesLookup,eventsLookup);
                    NewItemInHandSystem.ChangeItemInHand(ref state,item,hands);               
                    break;
                }
            entityCommandBuffer.DestroyEntity(rpc);
        }

        foreach ((RefRO<UnloadRPC> action,Entity rpc) in SystemAPI.Query<RefRO<UnloadRPC>>().WithEntityAccess())
        {    
            Debug.Log("jest!!");
            foreach((RefRW<Hands> hands,RefRO<GhostOwner> ghostOwner,Entity player) in SystemAPI.Query<RefRW<Hands>,RefRO<GhostOwner>>().WithAll<Player,ContainersLoaded>().WithEntityAccess())
            {
                if(ghostOwner.ValueRO.NetworkId != action.ValueRO.networkID) continue;
                if(ItemsAsset.instance.TryGetItem<RangedWeapon>(action.ValueRO.weaponID,out var item))
                {
                    float time = EntityHelper.TicksToSeconds(currentTime.ServerTick.TicksSince(action.ValueRO.tick));
                    state.EntityManager.SetComponentData<Cooldown>(player,new Cooldown(){ cooldownTick = EntityHelper.AddTime(action.ValueRO.tick,item.reloadCooldown)});
                    state.EntityManager.SetComponentData<CurrentPlayerState>(player,new CurrentPlayerState(){ state = PlayerState.unloading});
                    StartAnimation(ref state,item,hands,item.unloadAnim,animationLookup,transformLookup,framesLookup,eventsLookup,time,new int[]{action.ValueRO.ammoID});
                }
                break;
            }
            entityCommandBuffer.DestroyEntity(rpc);
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
    public static void StartAnimation(ref SystemState state,RangedWeapon item,RefRW<Hands> hands, List<KeyFrame> frames,
    ComponentLookup<AnimationComponent> animationLookup,ComponentLookup<LocalTransform> transformLookup,BufferLookup<AnimationFrames> framesLookup,BufferLookup<AnimationEvents> eventsLookup,float elapsedTime = 0,int[] args = null)
    {
        ResetAnimation(hands.ValueRO,animationLookup,transformLookup,framesLookup,eventsLookup);
        NewItemInHandSystem.ChangeItemInHand(ref state,item,hands);
                    
        int index = 0;
        foreach(var frame in frames)
        {
            var part = hands.ValueRO.GetBodyPart(frame.BodyPartType);
            if(frame.BodyPartType == BodyPartType.SideHand && item.twoHanded)
                animationLookup.GetRefRW(part).ValueRW.characterCenterPosition = -1 * transformLookup.GetRefRO(hands.ValueRO.GetBodyPart(BodyPartType.MainHand)).ValueRO.Position + new float3(0,-0.05f,0);
            
            animationLookup.GetRefRW(part).ValueRW.itemID = item.ID;
            animationLookup.GetRefRW(part).ValueRW.elapsedTime = elapsedTime;
            
            framesLookup[part].Add(frame.GetAnimationFrame(index));
            foreach (var eventFrame in frame.Events)
            {
                eventsLookup[part].Add(eventFrame.GetEvent(index,args));
            }

            state.EntityManager.SetComponentEnabled<AnimationIsPaused>(part,false);
            index++;
        }
    } 
    public static void ResetAnimation(Hands hands,ComponentLookup<AnimationComponent> animationLookup,ComponentLookup<LocalTransform> transformLookup,BufferLookup<AnimationFrames> framesLookup,BufferLookup<AnimationEvents> eventsLookup)
    {
        ClearAnimationComponent(hands.GetBodyPart(BodyPartType.MainHand),animationLookup,transformLookup,framesLookup,eventsLookup);
        ClearAnimationComponent(hands.GetBodyPart(BodyPartType.SideHand),animationLookup,transformLookup,framesLookup,eventsLookup);
    }
}


