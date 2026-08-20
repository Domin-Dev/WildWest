using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
partial struct StartSetUpSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        if(GameInfo.StartGame)
        {
            GameInfo data = GameInfo.instance;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            int HourDurationInTicks = (int)(WorldConfig.TimeConfig.HourDuration * 60f * NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate);
            EntityHelper.CreateEntityWithComponent(ecb, new TimeConfig()
            {
                DayDuration = WorldConfig.TimeConfig.HourDuration * 24f,
                HourDuration = WorldConfig.TimeConfig.HourDuration ,
                DayDurationInTicks = HourDurationInTicks * 24,
                HourDurationInTicks = HourDurationInTicks,
                SeasonDuration = WorldConfig.TimeConfig.SeasonDuration
            });
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        state.Enabled = false;
    }

}