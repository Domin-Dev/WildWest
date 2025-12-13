using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
partial struct StartSetUpServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        Debug.Log((GameInfo.instance == null)  + " " + GameInfo.instance?.startGame);
        if(GameInfo.instance != null && GameInfo.instance.startGame)
        {
            GameInfo data = GameInfo.instance;
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            EntityHelper.CreateEntityWithComponent<EnableConnectionTimeoutCheck>(ref ecb);
            EntityHelper.CreateEntityWithComponent(ref ecb,new ServerData()
            {
                hash = data.passHash.HasValue ? data.passHash.Value : "",
                isPassword =  data.passHash.HasValue,
                isHost = true,
                playersLimit = math.max(1,data.playerLimit),
                hostNetworkID = int.MinValue,
            });   
            EntityHelper.CreateEntityWithComponent(ref ecb,new MapSettings()
            
            {
                seed = GameInfo.instance.seed,
                widthInChunks = 10,
                heightInChunks = 10,
                playerRenderSize = 2,
                maxChunksPerClient = 30,
                maxLoadedChunksInTick = 40,
                loadedChunksInTickPerClient =  5
            });     
            EntityHelper.CreateEntityWithBuffer<LoadedChunks>(ref ecb);
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        state.Enabled = false;
    }

}