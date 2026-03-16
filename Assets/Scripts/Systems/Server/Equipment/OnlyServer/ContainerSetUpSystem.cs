using System;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;



[BurstCompile]
[UpdateAfter(typeof(GoInGameServerSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct ContainerSetUpSystem : ISystem
{
    EntityQuery containerQuery;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        containerQuery = SystemAPI.QueryBuilder().WithAll<PlayerContainer,ContainerComponent,GhostInstance>().WithAll<Simulate>().WithNone<ContainerLoaded>().Build();
        state.RequireForUpdate(containerQuery);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer entityCommandBuffer = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        state.Dependency = new ContainerJob()
        {
            ecb = entityCommandBuffer.AsParallelWriter()
        }
        .ScheduleParallel(containerQuery,state.Dependency);
    }


    [BurstCompile]
    [WithNone(typeof(ContainerLoaded))]
    public partial struct ContainerJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;

        public void Execute(Entity entity,in PlayerContainer containerPlayer, in ContainerComponent container,  in GhostInstance ghostInstance,  [EntityIndexInQuery] int sortKey)
        {     
            if(ghostInstance.ghostId == 0)
                return;

            ecb.AddComponent<SendToOwner>(sortKey,entity);       
            if(container.serverContainer)
            {
                ecb.AppendToBuffer(sortKey,containerPlayer.player,new GhostChildren()
                {
                    child = entity,
                    ghostID = ghostInstance.ghostId
                });
                ecb.AddComponent(sortKey,entity,new SynchronizeRelevancyWithParent()
                {
                    parent = containerPlayer.player
                });
            }
            ecb.AddComponent<ContainerLoaded>(sortKey,entity);
        }
    }
}

