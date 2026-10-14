using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;


[UpdateInGroup(typeof(SetUpSystemGroup))]
[BurstCompile]
partial struct StartSetUpSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        if(GameInfo.StartGame)
        {
            GameInfo data = GameInfo.instance;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            #region Time
            int HourDurationInTicks = (int)(WorldConfig.TimeConfig.HourDuration * 60f * NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate);
            EntityHelper.CreateEntityWithComponent(ecb, new TimeConfig()
            {
                DayDuration = WorldConfig.TimeConfig.HourDuration * 24f,
                HourDuration = WorldConfig.TimeConfig.HourDuration ,
                DayDurationInTicks = (uint)(HourDurationInTicks * 24),
                HourDurationInTicks = HourDurationInTicks,
                SeasonDuration = WorldConfig.TimeConfig.SeasonDuration
            });
            #endregion 

            #region Chat
            var messageCleanUp = new MessageCleanUp()
            {
                CleanupPeriod = EntityHelper.SecondsToTicks(MessageConfig.Instance.CleanupPeriod),
                MessageChunkLifetime = EntityHelper.SecondsToTicks(MessageConfig.Instance.MessageChunkLifetime)
            };
            EntityHelper.CreateEntityWithComponent(ecb,messageCleanUp,new RepeatAction(messageCleanUp.CleanupPeriod));
            
            var rPCCleanUp = new RPCCleanUp()
            {
                CleanupPeriod = EntityHelper.SecondsToTicks(MessageConfig.Instance.RPCCleanupPeriod),
                MaxRpcAgeFrames = MessageConfig.Instance.MaxRpcAgeFrames
            };
            EntityHelper.CreateEntityWithComponent(ecb,rPCCleanUp,new RepeatAction(rPCCleanUp.CleanupPeriod));      
            #endregion
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        state.Enabled = false;
    }

}