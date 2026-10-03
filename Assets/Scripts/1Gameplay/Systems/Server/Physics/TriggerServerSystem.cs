using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Utilities;



[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[UpdateBefore(typeof(AfterPhysicsSystemGroup))]
[UpdateAfter(typeof(TriggerSystem))]
public partial struct TriggerServerSystem : ISystem
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

        var job = new TriggerServerJob()
        {
            ecb = ecb,
            worldItemLookup = SystemAPI.GetComponentLookup<WorldItem>(),
            playerLookup = SystemAPI.GetComponentLookup<Player>(true),

            connectionLookup = SystemAPI.GetComponentLookup<PlayerSourceConnection>(true),
            ghostOwner = SystemAPI.GetComponentLookup<GhostOwner>(true),
            needLookup = SystemAPI.GetBufferLookup<PlayersNeedChunk>(true),
            loadedChunks = SystemAPI.GetSingletonBuffer<LoadedChunks>(true),
            tick = SystemAPI.GetSingleton<NetworkTime>().ServerTick,

            slotLookup = SystemAPI.GetBufferLookup<InventorySlot>(true),
            entityContainersLookup = SystemAPI.GetBufferLookup<EntityContainers>(true),
            contComponentLookup = SystemAPI.GetComponentLookup<ContainerComponent>(true),
        };
        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = job.Schedule(simulationSingleton, state.Dependency);
        ecbSystem.AddJobHandleForProducer(state.Dependency);
    }

}
public struct TriggerServerJob : ITriggerEventsJob
{
    [ReadOnly] public ComponentLookup<Player> playerLookup;
    public ComponentLookup<WorldItem> worldItemLookup;

    [ReadOnly] public ComponentLookup<PlayerSourceConnection> connectionLookup;
    [ReadOnly] public ComponentLookup<GhostOwner> ghostOwner;
    [ReadOnly] public BufferLookup<PlayersNeedChunk> needLookup;
    [ReadOnly] public DynamicBuffer<LoadedChunks> loadedChunks;
    [ReadOnly] public NetworkTick tick;


    [ReadOnly] public BufferLookup<InventorySlot>  slotLookup;
    [ReadOnly] public BufferLookup<EntityContainers>  entityContainersLookup;
    [ReadOnly] public ComponentLookup<ContainerComponent>  contComponentLookup;

    public EntityCommandBuffer ecb;
    public void Execute(TriggerEvent triggerEvent)
    {
        Entity player;
        Entity worldItem;

        if(playerLookup.HasComponent(triggerEvent.EntityA) && worldItemLookup.HasComponent(triggerEvent.EntityB))
        {
            player = triggerEvent.EntityA;
            worldItem = triggerEvent.EntityB;
        }
        else if(playerLookup.HasComponent(triggerEvent.EntityB) && worldItemLookup.HasComponent(triggerEvent.EntityA))
        {
            player = triggerEvent.EntityB;
            worldItem = triggerEvent.EntityA;   
        }
        else  
            return;

        if(!worldItemLookup.IsComponentEnabled(worldItem))
            return;


        WorldItem worldItemComponent = worldItemLookup[worldItem];
        if(EQHelper.FindSlotForItem(contComponentLookup,slotLookup,entityContainersLookup,player,worldItemComponent.item,out int remains).Length > 0)
        {
            int networkId = ghostOwner[player].NetworkId;
            RPCHelper.SendEventsToClientsAndOwner<PickUpItemRPC>(new PickUpItemRPC(worldItemComponent.chunkIndex,worldItemComponent.slotIndex,0.4f,remains == 0),connectionLookup,needLookup,loadedChunks,ecb,networkId,player,worldItemComponent.chunkIndex,tick,true);
            worldItemLookup.SetComponentEnabled(worldItem,false);     
            ecb.AddComponent<DestroyEntityTag>(worldItem);
        }
    }
}