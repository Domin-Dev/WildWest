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

                playerRenderSize = 1,
                maxChunksPerClient = 12,
            }); 



            EntityHelper.CreateEntityWithComponent(ref ecb,new UnloadingSettings()
            {
                loadedChunksPerPlayer = 15,
                chunkUnloadingPeriod = 10,
                maxIdleChunkTime = 10,
            });
            EntityHelper.CreateEntityWithComponent(ref ecb,new ShootingConfig()
            {
                maxSpread = 7f,
                shootSpread = 4f,
                changeItemInHandSpread = 2f,
                sensitivityPlayerAim = 3f,
                sensitivityPlayerMove = 1f,
                spreadRecovery = 0.06f,
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

            var maskItems = LayerMask.NameToLayer("WorldItems");
            var maskEnvironment = LayerMask.NameToLayer("Environment");
            EntityHelper.CreateEntityWithComponent(ecb, new WorldItemsConfig()
            {
                FilterToFindSimilarWorldItems = new Unity.Physics.CollisionFilter()
                {
                    BelongsTo = uint.MaxValue,
                    CollidesWith = (uint) math.pow(2,maskItems)
                },
                FilterToFindEnvironment = new Unity.Physics.CollisionFilter()
                {
                    BelongsTo = uint.MaxValue,
                    CollidesWith = (uint) math.pow(2,maskEnvironment)
                },
                sizeWorldItemCollider = new float2(0.23f,0.23f),
                dropRangeMin = 0.22f,
                dropRangeMax = 0.3f
            });

            EntityHelper.CreateEntityWithBuffer<LoadedChunks>(ecb);
            EntityHelper.CreateEntityWithBuffer<PlayersList>(ecb);

            EntityHelper.CreateEntityWithComponent(ecb,new LocalWeather());
            
            #region Time         
            EntityHelper.CreateEntityWithComponent(ecb, new CurrentTime()
            {
                Day = WorldConfig.TimeConfig.startDay,
                Hour = WorldConfig.TimeConfig.startHour,
                Season = WorldConfig.TimeConfig.startSeason,
                StartTick = NetworkTick.Invalid,
                StartHour = WorldConfig.TimeConfig.startHour,
                NextTimeOfDay = WorldConfig.TimeConfig.GetTimeOfDayThreshold(
                    WorldConfig.TimeConfig.startSeason,
                    WorldConfig.TimeConfig.startHour,
                    WorldConfig.TimeConfig.startDay,
                    out TimeOfDay currentTimeOfDay),
                TimeOfDay = currentTimeOfDay
            });
            #endregion

            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
        state.Enabled = false;
    }

}