using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
partial struct StartSetUpServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
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
                playersLimit = math.max(8,data.playerLimit),
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

                playerRenderSize = 2,
                maxChunksPerClient = 30,
            }); 



            EntityHelper.CreateEntityWithComponent(ref ecb,new UnloadingSettings()
            {
                loadedChunksPerPlayer = 15,
                chunkUnloadingPeriod = 10,
                maxIdleChunkTime = 10,
            });
            EntityHelper.CreateEntityWithComponent(ref ecb,new ShootingConfig()
            {
                maxSpread = 30f,
                shootSpread = 1f,
                sensitivityPlayerAim = 1f,
                sensitivityPlayerMove = 1f,
            });      
            EntityHelper.CreateEntityWithComponent(ecb, new TickLimitsConfig()
            {
                maxLoadedChunksInTick = 50,
                loadedChunksInTickPerClient = 10,

                maxStopRequestsInTick = 100,
                stopRequestsInTickPerClient = 10,

                maxStartRequestsInTick = 100,           
                startRequestsInTickPerClient = 10,

                maxUpdateChunkTimeRequestsInTick = 400,
                updateChunkTimeRequestsInTickPerClient = 30,

                maxUnloadedChunksInTick = 50,
                unloadedChunksInTickPerClient = 5,
            });
            EntityHelper.CreateEntityWithComponent(ecb, new SavesConfig()
            {
                savePeriod = 20,
                savedChunksInTickPerClient = 5,
                maxSavedChunksInTick = 50,
                defragmentationLimit = 0.6f
            });

            EntityHelper.CreateEntityWithBuffer<LoadedChunks>(ecb);
            EntityHelper.CreateEntityWithBuffer<PlayersList>(ecb);
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        state.Enabled = false;
    }

}