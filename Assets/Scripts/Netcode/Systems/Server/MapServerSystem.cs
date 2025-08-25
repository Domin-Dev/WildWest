using System;
using System.Collections.Generic;
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

    public Map map;
    private MapGenerator generator;
    public float2 spawnPoint;
    int simulationTickRate = 60;
    private const int ChunksPerTick = 2;
    private NetworkTick currentTick;
    private EntitiesReferences entitiesReferences;

    protected override void OnCreate()
    {
        RequireForUpdate<SendMap>();
        if (NetCodeConfig.Global != null) simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
    }
    protected override void OnDestroy()
    {
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

            for (int i = 0; i < map.chunks.Count; i++)
            {
                Entity chunk = entityCommandBuffer.CreateEntity();
                FixedChunk fixedChunk = new FixedChunk();
                GetChunk(i, ref fixedChunk);


                uint lifetimeInTicks = (uint)(i / ChunksPerTick);
                var targetTick = currentTick;
                targetTick.Add(lifetimeInTicks);
                entityCommandBuffer.AddComponent(chunk,fixedChunk);
                entityCommandBuffer.AddComponent(chunk, new RPCSendQueue() { target = entity, tick = targetTick });
            }


            GetChunkObjects(0, ref entityCommandBuffer, entity);
            GetChunkObjects(1, ref entityCommandBuffer, entity);
            GetChunkObjects(2, ref entityCommandBuffer, entity);
            GetChunkObjects(3, ref entityCommandBuffer, entity);


            Entity loaded = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent(loaded, new MapIsLoaded());
            entityCommandBuffer.AddComponent(loaded, new SendRpcCommandRequest()
            {
                TargetConnection = entity
            });

            entityCommandBuffer.RemoveComponent<SendMap>(entity);
        }
        entityCommandBuffer.Playback(this.EntityManager);
        entityCommandBuffer.Dispose();
    }
    private void GetChunk(int index, ref FixedChunk chunkStruct)
    {
        Chunk chunk = map.chunks[index];

        chunkStruct.worldPosition = chunk.worldPosition;
        chunkStruct.chunkCoordinates = new int2((int)chunk.chunkCoordinates.x, (int)chunk.chunkCoordinates.y);

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                chunkStruct[i, j] = new FixedTile(chunk.grid[i,j]);
            }
        }
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




