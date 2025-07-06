using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;
using System;
using Unity.Mathematics;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using Unity.Transforms;
using TMPro;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class MapServerSystem : SystemBase
{

    public Map map;
    private MapGenerator generator;
    public float2 spawnPoint;
    protected override void OnCreate()
    {
        RequireForUpdate<SendMap>();
    }
    public void GenerateMap()
    {
        generator = new MapGenerator(GameInfo.instance.seed);
        map = generator.StartGenerator();
        Debug.Log("Generowanie");
    }
    protected override void OnUpdate()
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach ((RefRO<SendMap> send,Entity entity) in
        SystemAPI.Query<RefRO<SendMap>>().WithEntityAccess())
        {
            if (map == null) 
            {
                GenerateMap();
            }

            for (int i = 0; i < map.chunks.Count; i++)
            {
                Entity chunk = entityCommandBuffer.CreateEntity();
                FixedChunk fixedChunk = new FixedChunk();
                GetChunk(i, ref fixedChunk);

                entityCommandBuffer.AddComponent(chunk, fixedChunk);
                entityCommandBuffer.AddComponent(chunk, new SendRpcCommandRequest()
                {
                    TargetConnection = entity
                });
            }

        //    GetChunkObjects(0,ref entityCommandBuffer, entity);
        //    GetChunkObjects(1,ref entityCommandBuffer, entity);

            Entity loaded = entityCommandBuffer.CreateEntity();
            entityCommandBuffer.AddComponent(loaded, new MapIsLoaded());
            entityCommandBuffer.AddComponent(loaded, new SendRpcCommandRequest()
            {
                TargetConnection = entity
            });

            entityCommandBuffer.RemoveComponent<SendMap>(entity);
            Debug.Log("Map is loaded");
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
                        SendBuidlingObjectRPC(ref entityCommandBuffer, fixedBuildingObjects, ref target);
                        fixedBuildingObjects = new FixedBuildingObjects();
                        counter = 0;
                    }
                    CreateObject(ref entityCommandBuffer, gridObject, new float2(i + chunk.chunkCoordinates.x, j + chunk.chunkCoordinates.y));
                }
            }
        }

            if (counter != 0) SendBuidlingObjectRPC(ref entityCommandBuffer, fixedBuildingObjects, ref target);
      
    }
    private void SendBuidlingObjectRPC(ref EntityCommandBuffer entityCommandBuffer, FixedBuildingObjects fixedBuildingObjects, ref Entity target)
    {
        var rpc = entityCommandBuffer.CreateEntity();
        entityCommandBuffer.AddComponent(rpc, fixedBuildingObjects);
        entityCommandBuffer.AddComponent(rpc, new SendRpcCommandRequest()
        {
            TargetConnection = target
        });
    }
    private void CreateObject(ref EntityCommandBuffer entityCommand,GridObject gridObject, float2 pos)
    {
        Entity entity = entityCommand.CreateEntity();
        LocalTransform localTransform = LocalTransform.FromPosition(new float3(pos.x, pos.y, pos.y));


        entityCommand.AddComponent(entity, localTransform);
    //    entityCommand.AddComponent(entity, new IsChanged());
    //    entityCommand.SetComponentEnabled(entity,typeof(IsChanged), true);
        entityCommand.AddComponent(entity, new Physics2D() {
            layer = 0,
            cellIndex = new int2(int.MinValue, int.MinValue)
        });
        entityCommand.AddComponent(entity, new BoxCollider2D()
        {
            offset = 0f,
            size = new float2(0.2f, 0.2f)
        });
        entityCommand.AddComponent(entity, new Velocity2D() { Value = float2.zero});
    }
}




