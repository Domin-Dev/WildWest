using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(UpdateNetworkTimeSystem))]
[UpdateAfter(typeof(SetUpSystemGroup))]
[BurstCompile]
partial struct ServerNetworkTimeSetUpSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var time = SystemAPI.GetSingleton<NetworkTime>();
        var ecbSingleton = SystemAPI.GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        if(SystemAPI.TryGetSingleton<ServerMessagesConfig>(out var config))
            EntityHelper.CreateEntityWithComponent(ecb,new MessageSystemCleanUp(),new RepeatAction(config.CleanupPeriod));


        if(SystemAPI.TryGetSingletonRW<CurrentTime>(out var currentTime))
        {
            currentTime.ValueRW.StartTick = time.ServerTick;
            EntityHelper.CreateEntityWithComponent(state.EntityManager,new NextWeatherUpdate(){ tick = time.ServerTick});
        }

        state.Enabled = false;
    }
} 