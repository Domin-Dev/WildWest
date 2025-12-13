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
        Debug.Log((GameInfo.instance == null)  + " " + GameInfo.instance?.startGame);
        if(GameInfo.instance != null && GameInfo.instance.startGame)
        {
            GameInfo data = GameInfo.instance;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            EntityHelper.CreateEntityWithComponent(ref ecb, new PlayerName() { name = data.playerName });
            EntityHelper.CreateEntityWithComponent<EnableConnectionTimeoutCheck>(ref ecb);            

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        state.Enabled = false;
    }

}