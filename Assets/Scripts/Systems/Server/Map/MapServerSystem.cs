
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

[UpdateAfter(typeof(PlayerMoveSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
partial struct MapServerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MapSettings>();
        state.RequireForUpdate<CurrentTime>();
        state.RequireForUpdate<LocalWeather>();
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        var map = SystemAPI.GetSingleton<MapSettings>();

        foreach ((RefRO<SendMap> send, Entity entity) in
        SystemAPI.Query<RefRO<SendMap>>().WithEntityAccess())
        {
            Entity loaded = ecb.CreateEntity();
            ecb.AddComponent(loaded, new StartDataRPC() 
            {
                mapSetUp = map.GetSetUp(),
                currentTime = SystemAPI.GetSingleton<CurrentTime>(),     
            });
            ecb.AddComponent(loaded, new SendRpcCommandRequest()
            {
                TargetConnection = entity
            });

            ecb.RemoveComponent<SendMap>(entity);
        }
        ecb.Playback(state.EntityManager);
    }
}

