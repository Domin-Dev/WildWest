
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public struct MapSettings: IComponentData
{
    public bool newMap;
    public int seed;



    /// Map Size
    public int mapSizeInRegions;
    public int regionSizeInChunks;
    public int chunkSizeInTiles;
    public float tileSize;
    public float2 mapOffset;


    public int mapSizeInChunks => mapSizeInRegions * regionSizeInChunks;
    public int chunksCount => mapSizeInChunks*mapSizeInChunks;
    public int regionsCount => mapSizeInRegions*mapSizeInRegions;
    public int tilesCount => chunkSizeInTiles*chunkSizeInTiles;

    // Server fields
    public int playerRenderCount => (2 * playerRenderSize + 1)*(2 * playerRenderSize + 1);
    public int playerRenderSize; 
    public int maxChunksPerClient;



    public int maxLoadedChunksInTick;
    public int loadedChunksInTickPerClient;




    [BurstCompile]
    public void GetNeighboringChunkIndexes(int chunkIndex,NativeHashMap<int,int> indexes)
    {
        int2 pos = GetChunkCoordinates(chunkIndex);
        for (int y = -playerRenderSize; y <= playerRenderSize; y++)
        {
            for (int x = -playerRenderSize; x <= playerRenderSize; x++)
            {
                if (pos.x + x >= 0 && pos.y + y >= 0 && pos.x + x < mapSizeInChunks && pos.y + y < mapSizeInChunks)
                {
                    int prio = math.abs(y) + math.abs(x);
                    indexes.Add(chunkIndex + x + y * mapSizeInChunks,prio);
                }
            }
        }
    }
    [BurstCompile]
    public int2 GetChunkCoordinates(int chunkIndex)
    {
        return new int2(chunkIndex % mapSizeInChunks, chunkIndex / mapSizeInChunks);
    }
    [BurstCompile]
    public int GetRegion(int2 chunkCoordinates)
    {
        return (int)(chunkCoordinates.x / mapSizeInRegions) + (int)(chunkCoordinates.y / mapSizeInRegions) * mapSizeInChunks;
    }
    [BurstCompile]
    public bool CheckChunkIndex(int chunkIndex)
    {
        return chunkIndex >= 0 && chunkIndex < chunksCount;
    }
    [BurstCompile]
    public int GetChunkIndex(float2 enginePosition)
    {
        return (int)(enginePosition.x / tileSize / chunkSizeInTiles) 
            + (int)(enginePosition.y / tileSize / chunkSizeInTiles) * mapSizeInChunks;
    }
    [BurstCompile]
    public int GetChunkIndex(float3 enginePosition)
    {
        return GetChunkIndex(MyTools.ConvertFloat(enginePosition));
    }
    [BurstCompile]
    public float3 MapPositionToWorldPosition(int x, int y)
    {
        return new float3(mapOffset.x + (x + 0.5f) * tileSize, mapOffset.y + (y + 0.5f) * tileSize, mapOffset.y + (y + 0.5f) * tileSize); 
    }
    [BurstCompile]
    public float2 GetChunkEnginePos(int2 chunkCoordinates)
    {
        return mapOffset + new float2(chunkCoordinates.x * chunkSizeInTiles * tileSize, chunkCoordinates.y * chunkSizeInTiles * tileSize);
    }
    [BurstCompile]
    public float2 GetChunkEnginePos(int chunkIndex)
    {
        return GetChunkEnginePos(GetChunkCoordinates(chunkIndex));
    }
    [BurstCompile]
    public int2 GetChunkMapPosition(int2 chunkCoordinates)
    {
        return chunkCoordinates * chunkSizeInTiles;
    }
    [BurstCompile]
    public int2 GetChunkMapPosition(int chunkIndex)
    {
        return GetChunkMapPosition(GetChunkCoordinates(chunkIndex));
    }
    [BurstCompile]
    public int2 GetTileLocalPos(int tileID)
    {
        return new int2(tileID % chunkSizeInTiles, tileID / chunkSizeInTiles);
    }
}