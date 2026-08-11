using System.Diagnostics;
using NUnit.Framework.Internal.Execution;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public struct CurrentTime : IComponentData
{
    public float Hour;
    public TimeOfDay TimeOfDay;
    public float NextTimeOfDay;
    public int Day;
    public Season season;


    public NetworkTick startTick;
    public float startHour;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ServerTimeSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CurrentTime>();
        state.RequireForUpdate<TimeConfig>();
        state.RequireForUpdate<NetworkTime>();

        if(SystemAPI.HasSingleton<CurrentTime>())
        {
            var time = SystemAPI.GetSingleton<NetworkTime>();
            SystemAPI.GetSingletonRW<CurrentTime>().ValueRW.startTick = time.ServerTick;
        }
    }
    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        var worldTime = SystemAPI.GetSingletonRW<CurrentTime>();
        var config = SystemAPI.GetSingleton<TimeConfig>();

        TimeService.UpdateCurrentTime(networkTime.ServerTick,worldTime,config,out int ticksSince,out bool nextDay,out bool nextSeason,out bool nextTimeOfDay);
    }
}

