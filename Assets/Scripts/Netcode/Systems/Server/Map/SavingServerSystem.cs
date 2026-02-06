using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEditor.Localization.Plugins.XLIFF.V20;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(MapSystemGroup),OrderLast = true)]
[RequireMatchingQueriesForUpdate]
[BurstCompile]
public partial class SavingServerSystem : SystemBase
{

    EntityQuery chunkQuery;
    EntityQuery playerQuery;
    NetworkTick nextUpdate;

    private static NativeList<(int region,ChunkSave chunk)> chunksToSaveRW;    
    public static NativeList<(int region,ChunkSave chunk)> chunksToSaveRO;   
    
    private static NativeList<PlayerSave> playersToSaveRW;   
    public static NativeList<PlayerSave> playersToSaveRO;   

    private static HeaderData? headerDataRW;
    public static HeaderData? headerDataRO;




    [BurstCompile]
    protected override void OnCreate()
    {
        chunkQuery = SystemAPI.QueryBuilder().WithAll<ChunkComponent,BuildingObjects,ChunkTiles>().WithAll<ToSave>().WithNone<NewChunk>().Build();
        playerQuery = SystemAPI.QueryBuilder().WithAll<Player>().WithAll<ToSave>().WithNone<NewChunk>().Build();
        
        nextUpdate = NetworkTick.Invalid;
        chunksToSaveRW = new NativeList<(int,ChunkSave)>(128,Allocator.Persistent);
        chunksToSaveRO = new NativeList<(int,ChunkSave)>(128,Allocator.Persistent);

        playersToSaveRW = new NativeList<PlayerSave>(16,Allocator.Persistent);
        playersToSaveRO = new NativeList<PlayerSave>(16,Allocator.Persistent);

        RequireForUpdate<SavesConfig>();
        if(SystemAPI.TryGetSingleton<MapSettings>(out var map) && SystemAPI.TryGetSingleton<SavesConfig>(out var config) )
            SaveIOThread.Start(map.chunksCountInRegion,config.defragmentationLimit);
    }

    [BurstCompile]
    protected override void OnDestroy()
    {
        SaveIOThread.Stop();
        chunksToSaveRO.Dispose();
        chunksToSaveRW.Dispose();
    }


    [BurstCompile]
    protected override void OnUpdate()
    {
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        if(nextUpdate.IsValid && !currentTick.IsNewerThan(nextUpdate)) 
            return;


        var settings = SystemAPI.GetSingleton<SavesConfig>();
        SetTimer(currentTick,settings.savePeriod);


        var map = SystemAPI.GetSingleton<MapSettings>();
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged);

        foreach ((RefRO<ChunkComponent> chunkComp,DynamicBuffer<ChunkTiles> tiles,DynamicBuffer<BuildingObjects> buildingObjects, Entity entity) in SystemAPI.Query<RefRO<ChunkComponent>,DynamicBuffer<ChunkTiles>,DynamicBuffer<BuildingObjects>>().WithAll<ToSave>().WithNone<NewChunk>().WithEntityAccess())
        {
            NativeArray<TileSave> tileDatas = new NativeArray<TileSave>(tiles.Length,Allocator.Persistent);
            NativeArray<BuildingObjectSave> buildingObjs = new NativeArray<BuildingObjectSave>(buildingObjects.Length,Allocator.Persistent);
            for (int i = 0; i < tiles.Length; i++)
                tileDatas[i] = new TileSave(tiles[i]);

            for (int i = 0; i < buildingObjects.Length; i++)
                buildingObjs[i] = new BuildingObjectSave(buildingObjects[i]);  
            

            chunksToSaveRW.Add((chunkComp.ValueRO.regionIndex,new ChunkSave()
            {
                chunkIndex = chunkComp.ValueRO.chunkIndex,
                localChunkIndex = map.GetRegionChunkIndex(chunkComp.ValueRO.chunkIndex),
                tiles = tileDatas,
                objects = buildingObjs
            }));
            ecb.SetComponentEnabled<ToSave>(entity,false);
        }

        foreach ((RefRO<Player> player,RefRO<PlayerSourceConnection> connection,RefRO<Health> health,RefRO<Hunger> hunger, RefRO<Thirst> thirst, RefRO<PlayerLook> look,RefRO<LocalTransform> pos,Entity entity) in 
        SystemAPI.Query<RefRO<Player>,RefRO<PlayerSourceConnection>,RefRO<Health>,RefRO<Hunger>, RefRO<Thirst>,RefRO<PlayerLook>,RefRO<LocalTransform>>().WithAll<ToSave>().WithNone<NewPlayerTag>().WithEntityAccess())
        {
            PlayerSave playerSave = new PlayerSave();
            playerSave.isAdmin = SystemAPI.HasComponent<Admin>(connection.ValueRO.value);

            playerSave.playerName = player.ValueRO.playerName;
            playerSave.characterLook = look.ValueRO.look;
            playerSave.playerPosition = new Unity.Mathematics.float2(pos.ValueRO.Position.x,pos.ValueRO.Position.y);

            playerSave.health = health.ValueRO.Value;
            playerSave.hunger = hunger.ValueRO.Value;
            playerSave.thirst = thirst.ValueRO.Value;
            
            playersToSaveRW.Add(playerSave);
            ecb.SetComponentEnabled<ToSave>(entity,false);
        }
        
        headerDataRW = GameInfo.instance.GetHeader();
        Swap();
    }

    [BurstCompile]
    private  void SetTimer(NetworkTick currentTick,int time)
    {
        var simulationTickRate = 60;
        if (NetCodeConfig.Global != null) simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;

        uint lifetimeInTicks = (uint)(time* simulationTickRate);
        nextUpdate = currentTick;
        nextUpdate.Add(lifetimeInTicks);
    }

    [BurstCompile]
    private void Swap()
    {
        SawpContainers(ref chunksToSaveRO,ref chunksToSaveRW);
        SawpContainers(ref playersToSaveRO,ref playersToSaveRW);
        if(headerDataRO == null)
            headerDataRO = headerDataRW;
        
    }
    
    [BurstCompile]
    private void SawpContainers<T>(ref NativeList<T> RO,ref NativeList<T> RW) where T : unmanaged
    {
        if(RO.IsEmpty && RW.Length > 0)
        {
            var tmp = RW;
            RW = RO;
            RO = tmp;
            SaveIOThread.Notify();
        }
    }
}

   