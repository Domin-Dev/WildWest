
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[BurstCompile]
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

    public float chunkSizeInEnginePos => chunkSizeInTiles * tileSize;

#region Chunks limits
    public int playerRenderCount => (2 * playerRenderSize + 1)*(2 * playerRenderSize + 1);
    public int playerRenderSize; 
    public int maxChunksPerClient;

#endregion

#region Limits for ticks
    public int maxLoadedChunksInTick;
    public int loadedChunksInTickPerClient;

    public int maxStopRequestsInTick;
    public int stopRequestsInTickPerClient;

    public int maxStartRequestsInTick;
    public int startRequestsInTickPerClient;
#endregion



    [BurstCompile]
    public bool CheckChunkIndex(int chunkIndex)
    {
        return chunkIndex >= 0 && chunkIndex < chunksCount;
    }

    
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
    public int GetChunkIndexFromCoordinates(int2 coordinates)
    {
        return coordinates.x + coordinates.y * mapSizeInChunks;
    }
    [BurstCompile]
    public int GetChunkIndexFromEnginePosition(float2 enginePosition)
    {
        if(enginePosition.x >= 0 && enginePosition.y >= 0)
            return (int)(enginePosition.x / tileSize / chunkSizeInTiles) 
                + (int)(enginePosition.y / tileSize / chunkSizeInTiles) * mapSizeInChunks;
        else
            return -1;
    }
    [BurstCompile]
    public int GetChunkIndexFromEnginePosition(float3 enginePosition)
    {
        return GetChunkIndexFromEnginePosition(MyTools.ConvertFloat(enginePosition));
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
    public int2 GetLocalTilePos(int tileID)
    {
        return new int2(tileID % chunkSizeInTiles, tileID / chunkSizeInTiles);
    }

    [BurstCompile]
    public int2 GetGlobalTilePos(int2 tileLocalPos,int2 chunkCoordinates)
    {
        return chunkCoordinates * chunkSizeInTiles + tileLocalPos;
    }
}