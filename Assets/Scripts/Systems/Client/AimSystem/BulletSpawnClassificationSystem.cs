using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.NetCode.LowLevel;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(GhostSpawnClassificationSystemGroup))]
[UpdateAfter(typeof(GhostSpawnClassificationSystem))]
[CreateAfter(typeof(GhostReceiveSystem))]
[CreateAfter(typeof(GhostCollectionSystem))]

[BurstCompile]
public partial struct GrenadeClassificationSystem : ISystem
{
    SnapshotDataLookupHelper snapshotDataLookupHelper;
    BufferLookup<PredictedGhostSpawn> predictedGhostSpawnLookup;
    ComponentLookup<Bullet> bulletLookup;

    int ghostType;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        snapshotDataLookupHelper = new SnapshotDataLookupHelper(ref state,
            SystemAPI.GetSingletonEntity<GhostCollection>(),
            SystemAPI.GetSingletonEntity<SpawnedGhostEntityMap>());

        predictedGhostSpawnLookup = state.GetBufferLookup<PredictedGhostSpawn>();
        bulletLookup = state.GetComponentLookup<Bullet>();
        state.RequireForUpdate<GhostSpawnQueue>();
        state.RequireForUpdate<PredictedGhostSpawnList>();
        state.RequireForUpdate<NetworkId>();
        state.RequireForUpdate<EntitiesReferences>();
        ghostType = -1;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (ghostType == -1)
        {
            var prefabEntity = SystemAPI.GetSingleton<EntitiesReferences>().bulletEntity;
            var collectionEntity = SystemAPI.GetSingletonEntity<GhostCollection>();
            var ghostPrefabTypes = state.EntityManager.GetBuffer<GhostCollectionPrefab>(collectionEntity);
            for (int i = 0; i < ghostPrefabTypes.Length; ++i)
            {
                if (ghostPrefabTypes[i].GhostPrefab == prefabEntity)
                {
                    ghostType = i;
                    break;
                }
            }
        }
        snapshotDataLookupHelper.Update(ref state);
        predictedGhostSpawnLookup.Update(ref state);
        bulletLookup.Update(ref state);

        var classificationJob = new BulletClassificationJob
        {
            snapshotDataLookupHelper = snapshotDataLookupHelper,
            spawnListEntity = SystemAPI.GetSingletonEntity<PredictedGhostSpawnList>(),
            PredictedSpawnListLookup = predictedGhostSpawnLookup,
            bulletLookup = bulletLookup,
            ghostType = ghostType
        };
        state.Dependency = classificationJob.Schedule(state.Dependency);

    }

    [WithAll(typeof(GhostSpawnQueue))]
    [BurstCompile]
    partial struct BulletClassificationJob : IJobEntity
    {
        public SnapshotDataLookupHelper snapshotDataLookupHelper;
        public Entity spawnListEntity;
        public BufferLookup<PredictedGhostSpawn> PredictedSpawnListLookup;
        public ComponentLookup<Bullet> bulletLookup;
        public int ghostType;

        public void Execute(DynamicBuffer<GhostSpawnBuffer> newSpawns, DynamicBuffer<SnapshotDataBuffer> data)
        {
            var predictedSpawnList = PredictedSpawnListLookup[spawnListEntity];
            var snapshotDataLookup = snapshotDataLookupHelper.CreateSnapshotBufferLookup();
            for (int i = 0; i < newSpawns.Length; ++i)
            {
                var newGhostSpawn = newSpawns[i];
                if (newGhostSpawn.GhostType != ghostType)
                    continue; 

                if (newGhostSpawn.SpawnType != GhostSpawnBuffer.Type.Predicted || newGhostSpawn.PredictedSpawnEntity != Entity.Null)
                    continue;

                newGhostSpawn.HasClassifiedPredictedSpawn = true;


                for (int j = 0; j < predictedSpawnList.Length; ++j)
                {
                    if (newGhostSpawn.GhostType == predictedSpawnList[j].ghostType)
                    {
                        if (snapshotDataLookup.TryGetComponentDataFromSnapshotHistory(newGhostSpawn.GhostType, data, out Bullet grenadeData, i))
                        {
                            var spawnIdFromList = bulletLookup[predictedSpawnList[j].entity].bulletID;
                            if (grenadeData.bulletID == spawnIdFromList)
                            {
                                newGhostSpawn.PredictedSpawnEntity = predictedSpawnList[j].entity;
                                predictedSpawnList.RemoveAtSwapBack(j);
                                break;
                            }
                        }
                    }
                }
                newSpawns[i] = newGhostSpawn;
            }
        }
    }
}
