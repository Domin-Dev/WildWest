using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
partial struct StartSetUpClientSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        if(GameInfo.instance != null && GameInfo.instance.startGame)
        {
            GameInfo data = GameInfo.instance;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            EntityHelper.CreateEntityWithComponent(ref ecb, new PlayerName() { name = data.playerName });
            EntityHelper.CreateEntityWithComponent<EnableConnectionTimeoutCheck>(ref ecb);   
            EntityHelper.CreateEntityWithComponent(ref ecb,new ShootingConfig()
            {
                maxSpread = 30f,
                shootSpread = 1f,
                sensitivityPlayerAim = 1f,
                sensitivityPlayerMove = 1f,
            });        

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        state.Enabled = false;
    }

}