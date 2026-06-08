using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;


[UpdateAfter(typeof(CollisionSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup))]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
partial struct CalculateChunksForBulletsServerSystem : ISystem
{
    EntityQuery bulletsQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        bulletsQuery =  SystemAPI.QueryBuilder().WithAll<LocalTransform,PhysicsVelocity,Bullet>().WithNone<ProcessedBullet>().WithDisabled<NewBullet>().Build();
        state.RequireForUpdate<MapSettings>();
        state.RequireForUpdate(bulletsQuery);
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        var mapSettings = SystemAPI.GetSingleton<MapSettings>();
        var loadedChunks =  SystemAPI.GetSingletonBuffer<LoadedChunks>();

        state.Dependency = new CalculateChunksForBulletsJob()
        {
            map = mapSettings,
            loadedChunks = loadedChunks,
            ecb = ecb,
            time = SystemAPI.Time.ElapsedTime           
        }
        .ScheduleParallel(bulletsQuery,state.Dependency);
    }


    [BurstCompile]

    public partial struct CalculateChunksForBulletsJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public MapSettings map;
        [ReadOnly] public double time;
        [ReadOnly] public DynamicBuffer<LoadedChunks> loadedChunks;

        [BurstCompile]
        public void Execute(Entity bulletEntity,in LocalTransform localTransform,in Bullet bullet, [EntityIndexInQuery] int sortKey)
        {
            NativeQueue<int2> chunks = new NativeQueue<int2>(Allocator.TempJob);
            NextChunks(MyTools.ConvertFloat(localTransform.Position),MyTools.ConvertFloat(localTransform.Right()),bullet.range,map.chunkSizeInEnginePos,chunks);
            int prio = 0;
            while(chunks.TryDequeue(out int2 chunk))
            {
                int chunkIndex = map.GetChunkIndexFromCoordinates(chunk);
                if(!map.CheckChunkCoordinates(chunk)) break;

                bool isLoaded = false;
                foreach(LoadedChunks item in loadedChunks)
                {
                    if(item.chunkIndex == chunkIndex)
                    {
                        ecb.SetComponent(sortKey,item.chunkEntity, new ChunkTimestamp(){ timestamp = time });
                        isLoaded = true;
                        break;
                    }
                }
                if(isLoaded) continue;
                var entity = ecb.CreateEntity(sortKey);
                ecb.AddComponent(sortKey,entity,new LoadChunkRequest()
                {
                    chunkIndex = chunkIndex,
                    networkID = -1,
                    playerEntity = Entity.Null,
                    priority = prio,
                });
                prio++;
            }
            ecb.AddComponent<ProcessedBullet>(sortKey,bulletEntity);
            chunks.Dispose();
        } 
          
        [BurstCompile]
        private void NextChunks(float2 position,float2 dir, float maxDistance,float chunkSize,NativeQueue<int2> requests)
        {
            dir = math.normalize(dir);

            int2 chunk = (int2)math.floor(position / chunkSize);

            int2 step = new int2(
                dir.x > 0 ? 1 : (dir.x < 0 ? -1 : 0),
                dir.y > 0 ? 1 : (dir.y < 0 ? -1 : 0)
            );

            float2 tDelta = new float2(
                dir.x != 0 ? math.abs(chunkSize / dir.x) : float.MaxValue,
                dir.y != 0 ? math.abs(chunkSize / dir.y) : float.MaxValue
            );

            int2 boundaryChunk = chunk + new int2(
                step.x > 0 ? 1 : 0,
                step.y > 0 ? 1 : 0
            );

            float2 nextBoundary = (float2)boundaryChunk * chunkSize;

            float2 tMax = new float2(
                dir.x != 0 ? (nextBoundary.x - position.x) / dir.x : float.MaxValue,
                dir.y != 0 ? (nextBoundary.y - position.y) / dir.y : float.MaxValue
            );

            requests.Enqueue(chunk);
            float traveled = 0f;
            while (traveled <= maxDistance)
            {
                if (tMax.x < tMax.y)
                {
                    chunk.x += step.x;
                    traveled = tMax.x;
                    tMax.x += tDelta.x;
                }
                else
                {
                    chunk.y += step.y;
                    traveled = tMax.y;
                    tMax.y += tDelta.y;
                }
                requests.Enqueue(chunk);
            }
        }
    }
}