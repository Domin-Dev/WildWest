using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
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
                newMap = true,
                mapSizeInRegions = 3,
                regionSizeInChunks = 5,
                chunkSizeInTiles = 10,

                mapOffset = new Vector2(0,0),
                tileSize = 0.25f,

                playerRenderSize = 1,
                maxChunksPerClient = 12,


                maxLoadedChunksInTick = 50,
                loadedChunksInTickPerClient = 10,

                maxStopRequestsInTick = 100,
                stopRequestsInTickPerClient = 10,

                maxStartRequestsInTick = 100,           
                startRequestsInTickPerClient = 10,
                
            });     
            EntityHelper.CreateEntityWithBuffer<LoadedChunks>(ref ecb);
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        state.Enabled = false;
    }

}