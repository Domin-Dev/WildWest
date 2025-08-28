using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class MapServerSystem : SystemBase
{
    private static Map map;
    private MapGenerator generator;
    public float2 spawnPoint;
    int simulationTickRate = 60;
    private const int ChunksPerTick = 2;
    private const int renderChunksSize = 2;

    private NetworkTick currentTick;
    private EntitiesReferences entitiesReferences;

    private NativeHashMap<int,Entity> loadedChunks;
    private NativeHashMap<int, NativeHashMap<int,int>> playerChunks;

    public static Map Map { get { return map; } }

    protected override void OnCreate()
    {
        if (NetCodeConfig.Global != null) simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
        loadedChunks = new NativeHashMap<int, Entity>(100, Allocator.Persistent);
        playerChunks = new NativeHashMap<int, NativeHashMap<int,int>>(50, Allocator.Persistent);
    }
    protected override void OnDestroy()
    {
        loadedChunks.Dispose();
        playerChunks.Dispose();
        base.OnDestroy();
    }


    public void GenerateMap()
    {
        generator = new MapGenerator(GameInfo.instance.seed);
        map = generator.StartGenerator();
    }
    protected override void OnUpdate()
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        foreach ((RefRO<SendMap> send,Entity entity) in
        SystemAPI.Query<RefRO<SendMap>>().WithEntityAccess())
        {
            if (map == null) 
            {
                Debug.Log("new map!!!");
                GenerateMap();
            }


            //for (int i = 0; i < map.chunks.Count; i++)
            //{
            //    Entity chunk = entityCommandBuffer.CreateEntity();
            //    FixedChunk fixedChunk = new FixedChunk();
            //    GetChunk(i, ref fixedChunk);


            //    uint lifetimeInTicks = (uint)(i / ChunksPerTick);
            //    var targetTick = currentTick;
            //    targetTick.Add(lifetimeInTicks);
            //    entityCommandBuffer.AddComponent(chunk, fixedChunk);
            //    entityCommandBuffer.AddComponent(chunk, new RPCSendQueue() { target = entity, tick = targetTick });
            //}


            //GetChunkObjects(0, ref entityCommandBuffer, entity);
            //GetChunkObjects(1, ref entityCommandBuffer, entity);
            //GetChunkObjects(2, ref entityCommandBuffer, entity);
            //GetChunkObjects(3, ref entityCommandBuffer, entity);


            Entity loaded = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent(loaded, new MapIsLoaded() {  widthInChunks = map.widthInChunks});
            entityCommandBuffer.AddComponent(loaded, new SendRpcCommandRequest()
            {
                TargetConnection = entity
            });

            entityCommandBuffer.RemoveComponent<SendMap>(entity);
        }
        


        foreach ((RefRO<LastChunk> chunk,RefRO<GhostOwner> networkID, Entity entity) in
        SystemAPI.Query<RefRO<LastChunk>,RefRO<GhostOwner>>().WithAll<NeedChunks>().WithEntityAccess())
        {
            var chunksToSend = map.GetNeighboringChunkIndexes(chunk.ValueRO.value,renderChunksSize);

            Debug.Log("new Chunk");

            for (int i = 0; i < chunksToSend.Count; i++)
            {
                int index = chunksToSend[i];
                if (PlayerHasChunk(networkID.ValueRO.NetworkId, index)) continue;
                Entity chunkEntity;

                if (!loadedChunks.TryGetValue(index, out chunkEntity))
                {
                    CreateChunk(ref entityCommandBuffer, index);
                    chunkEntity = loadedChunks[index];
                }

                playerChunks[networkID.ValueRO.NetworkId].Add(index,0);
                entityCommandBuffer.SetComponentEnabled<SendChunk>(chunkEntity, true);
                entityCommandBuffer.AppendToBuffer(chunkEntity,new ChunkRecipients()
                {
                    networkID = networkID.ValueRO.NetworkId
                });
            }

            entityCommandBuffer.SetComponentEnabled<NeedChunks>(entity, false);
        }

        entityCommandBuffer.Playback(this.EntityManager);
        entityCommandBuffer.Dispose();
    }



    private bool PlayerHasChunk(int networkID, int chunkIndex)
    {
        if(playerChunks.ContainsKey(networkID))
        {
            var chunks = playerChunks[networkID];
            return chunks.ContainsKey(chunkIndex);
        }
        else
        {
            playerChunks.Add(networkID, new NativeHashMap<int,int>((int)math.pow(renderChunksSize + 1,2), Allocator.Persistent));
        }
        return false;
    }
    private void CreateChunk(ref EntityCommandBuffer entityCommandBuffer,int index)
    {
        var entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        Entity chunkEntity = ClientServerBootstrap.ServerWorld.EntityManager.Instantiate(entitiesReferences.chunkEntity);
        var tiles = SystemAPI.GetBuffer<ChunkTiles>(chunkEntity);
        ChunkComponent chunkComponent = new ChunkComponent();
        entityCommandBuffer.AddComponent<SendChunk>(chunkEntity);
        entityCommandBuffer.AddBuffer<ChunkRecipients>(chunkEntity);

        Chunk chunk = map.chunks[index];
        chunkComponent.worldPos = chunk.worldPosition;
        chunkComponent.index = index;

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                GridTile tile = chunk.grid[j, i];
                tiles.Add(new ChunkTiles()
                {
                    tileID = tile.tileID,
                    variant = (byte)tile.variant
                });
            }
        }

        entityCommandBuffer.SetComponent(chunkEntity,chunkComponent);
        loadedChunks.Add(index, chunkEntity);
    }

    private void SendToPlayer(int index, ref FixedChunk chunkStruct)
    {

    }
    private void GetChunkObjects(int index, ref EntityCommandBuffer entityCommandBuffer,Entity target)
    {
        Chunk chunk = map.chunks[index];

        FixedBuildingObjects fixedBuildingObjects = new FixedBuildingObjects();
        fixedBuildingObjects.chunkCoordinates = new int2((int)chunk.chunkCoordinates.x, (int)chunk.chunkCoordinates.y);
        Entity rpc;

        int counter = 0;

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                GridObject gridObject = chunk.grid[i, j].gridObject;

                if (gridObject != null)
                {
                    byte[] bytes = gridObject.GetBytes();
                    List<byte> posXY = new List<byte>();
                    posXY.AddRange(BitConverter.GetBytes(i));
                    posXY.AddRange(BitConverter.GetBytes(j));

                    if(bytes.Length + 8 < FixedBuildingObjects.size - counter)
                    {
                        fixedBuildingObjects[counter] = (byte)bytes.Length;
                        counter++;
                        for (int l = 0; l < posXY.Count; l++)
                        {
                            fixedBuildingObjects[counter] = posXY[l];
                            counter++;
                        }
                        for (int k = 0; k < bytes.Length; k++)
                        {
                            fixedBuildingObjects[counter] = bytes[k];
                            counter++;
                        }
                    }
                    else
                    {
                        SendBuidlingObjectRPC(1,ref entityCommandBuffer, fixedBuildingObjects, ref target);
                        fixedBuildingObjects = new FixedBuildingObjects();
                        counter = 0;
                    }

                    
                    BuildingObjectCreator.CreateObject(ref entitiesReferences,EntityManager,ref entityCommandBuffer, gridObject, new float2(i + chunk.chunkCoordinates.x, j + chunk.chunkCoordinates.y));
                }
            }
        }

        if (counter != 0) SendBuidlingObjectRPC(index,ref entityCommandBuffer, fixedBuildingObjects, ref target);

    }
    private void SendBuidlingObjectRPC(int i,ref EntityCommandBuffer entityCommandBuffer, FixedBuildingObjects fixedBuildingObjects, ref Entity target)
    {
        var rpc = entityCommandBuffer.CreateEntity();
        entityCommandBuffer.AddComponent(rpc, fixedBuildingObjects);

        uint lifetimeInTicks = (uint)(i / ChunksPerTick);
        var targetTick = currentTick;
        targetTick.Add(lifetimeInTicks);
        entityCommandBuffer.AddComponent(rpc, new RPCSendQueue() { target = target, tick = targetTick });
    }
    
}




