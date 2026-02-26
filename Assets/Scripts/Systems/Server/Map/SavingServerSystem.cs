using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


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

    private static NativeList<(FixedString128Bytes playerName,ContainerSave container)> containersToSaveRW;   
    public static NativeList<(FixedString128Bytes playerName,ContainerSave container)> containersToSaveRO;   

    private static HeaderSave? headerDataRW;
    public static HeaderSave? headerDataRO;






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

        containersToSaveRW = new NativeList<(FixedString128Bytes playerName, ContainerSave container)>(64,Allocator.Persistent);
        containersToSaveRO = new NativeList<(FixedString128Bytes playerName, ContainerSave container)>(64,Allocator.Persistent);

        headerDataRO = null;
        headerDataRW = null;

        RequireForUpdate<SavesConfig>();
        if(SystemAPI.TryGetSingleton<MapSettings>(out var map) && SystemAPI.TryGetSingleton<SavesConfig>(out var config))
        {
            SaveIOThread.Create(map.chunksCountInRegion,config.defragmentationLimit);
            var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
            SetTimer(currentTick,config.savePeriod);
        }
    }

    [BurstCompile]
    protected override void OnDestroy()
    {
        SaveIOThread.Stop();
        chunksToSaveRO.Dispose();
        chunksToSaveRW.Dispose();

        playersToSaveRO.Dispose();
        playersToSaveRW.Dispose();

        containersToSaveRO.Dispose();
        containersToSaveRW.Dispose();
    }


    [BurstCompile]
    protected override void OnUpdate()
    {
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        if(nextUpdate.IsValid && !currentTick.IsNewerThan(nextUpdate)) 
            return;


        var settings = SystemAPI.GetSingleton<SavesConfig>();
        SetTimer(currentTick,settings.savePeriod);
        Save();
    }

    public void Save()
    {
        var map = SystemAPI.GetSingleton<MapSettings>();
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(EntityManager.WorldUnmanaged);
        var playersList = SystemAPI.GetSingletonBuffer<PlayersList>(true);

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
    
        foreach ((RefRO<GhostOwner> owner,RefRO<ContainerComponent> containerComponent,DynamicBuffer<InventorySlot> slots,DynamicBuffer<ItemBarData> barData, Entity entity) in SystemAPI.Query<RefRO<GhostOwner>,RefRO<ContainerComponent>,DynamicBuffer<InventorySlot>,DynamicBuffer<ItemBarData>>().WithAll<ToSave>().WithEntityAccess())
        {
            NativeArray<SlotSave> slotsArray = new NativeArray<SlotSave>(containerComponent.ValueRO.capacity,Allocator.Persistent);
            NativeArray<BarDataSave> itemBarData = new NativeArray<BarDataSave>(containerComponent.ValueRO.capacity,Allocator.Persistent);
        
            for(int i = 0; i < slotsArray.Length; i++)
            {
                slotsArray[i] = new SlotSave(){ itemId = -1};
                itemBarData[i] = new BarDataSave(){ value = -1};
            }
            foreach(var slot in slots)
                slotsArray[slot.slot] = new SlotSave(slot);

            foreach(var data in barData)
                itemBarData[data.slot] = new BarDataSave(data);

            foreach(var player in playersList)
            {
                if(player.networkID == owner.ValueRO.NetworkId)
                {
                    containersToSaveRW.Add((player.playerName,new ContainerSave()
                    {
                        containerIndex = containerComponent.ValueRO.containerIndex,
                        capacity = containerComponent.ValueRO.capacity,
                        slots = slotsArray,
                        barData = itemBarData
                    }));
                }
            } 
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
        SwapContainers(ref chunksToSaveRO,ref chunksToSaveRW);
        SwapContainers(ref playersToSaveRO,ref playersToSaveRW);
        SwapContainers(ref containersToSaveRO,ref containersToSaveRW);
        if(headerDataRO == null)
            headerDataRO = headerDataRW;
        
    }
    
    [BurstCompile]
    private void SwapContainers<T>(ref NativeList<T> RO,ref NativeList<T> RW) where T : unmanaged
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

   