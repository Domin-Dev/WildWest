using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;
using System;
using Unity.Mathematics;


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
        Debug.Log(map);
        Debug.Log(map.chunks);
        Debug.Log(map.chunks.Count);
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach ((RefRO<SendMap> send,Entity entity) in
        SystemAPI.Query<RefRO<SendMap>>().WithEntityAccess())
        {
            Debug.Log("mapaa ");
            Debug.Log(map);
            Debug.Log(generator);
            if (map == null) 
            {
                GenerateMap();
            }


            for (int i = 0; i < map.chunks.Count; i++)
            {
                Entity chunk = entityCommandBuffer.CreateEntity();
                FixedChunk fixedChunk = new FixedChunk();
                GetChunk(i, ref fixedChunk);

                //   entityCommandBuffer.AddComponent(chunk, fixedTileRow);
                entityCommandBuffer.AddComponent(chunk, fixedChunk);
                entityCommandBuffer.AddComponent(chunk, new SendRpcCommandRequest()
                {
                    TargetConnection = entity
                });
            }

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
        Debug.Log("dziala");
        Chunk chunk = map.chunks[index];
        Debug.Log("chunki dzalaja");

        chunkStruct.worldPosition = chunk.worldPosition;
        chunkStruct.chunkCoordinates = new int2((int)chunk.chunkCoordinates.x, (int)chunk.chunkCoordinates.y);

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {

                chunkStruct[i, j] = new FixedTile(chunk.grid[i,j]);

                if (chunk.grid[i,j].gridObject != null)
                {
               //     Debug.Log((chunk.grid[i, j].gridObject));
                }
            }
        }
    }
}



