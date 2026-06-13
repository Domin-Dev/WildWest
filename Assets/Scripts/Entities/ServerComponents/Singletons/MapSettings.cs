
using System;
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


    public float mapSizeInEngine => mapSizeInChunks * chunkSizeInEnginePos;
    public int mapSizeInChunks => mapSizeInRegions*regionSizeInChunks;

    
    
    public int chunksCountInRegion => regionSizeInChunks * regionSizeInChunks;
    public int chunksCount => mapSizeInChunks*mapSizeInChunks;
    public int regionsCount => mapSizeInRegions*mapSizeInRegions;
    public int tilesCount => chunkSizeInTiles*chunkSizeInTiles;

    public float chunkSizeInEnginePos => chunkSizeInTiles * tileSize;

    #region Chunks limits
    public int playerRenderCount => (2 * playerRenderSize + 1)*(2 * playerRenderSize + 1);
    public int playerRenderSize; 
    public int maxChunksPerClient;

    #endregion


    [BurstCompile]
    public bool CheckChunkIndex(int chunkIndex)
    {
        return chunkIndex >= 0 && chunkIndex < chunksCount;
    }

    [BurstCompile]
    public bool CheckChunkCoordinates(int2 chunkCoordinates)
    {
        return chunkCoordinates.x >= 0 && chunkCoordinates.y >= 0  && chunkCoordinates.x < mapSizeInChunks && chunkCoordinates.y < mapSizeInChunks;
    }
    [BurstCompile]
    public void GetNeighboringChunkIndexes(int chunkIndex,NativeHashMap<int,int> indexes)
    {
        int2 pos = GetChunkCoordinates(chunkIndex);
        for (int y = -playerRenderSize; y <= playerRenderSize; y++)
        {
            for (int x = -playerRenderSize; x <= playerRenderSize; x++)
            {
                int newX = pos.x + x;
                int newY = pos.y + y;

                if (newX >= 0 && newY >= 0 && newX < mapSizeInChunks && newY < mapSizeInChunks)
                {
                    int index = chunkIndex + x + y * mapSizeInChunks;
                    int prio = math.abs(y) + math.abs(x);
                    indexes.Add(index,prio);
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
        return (int)(chunkCoordinates.x / regionSizeInChunks) + (int)(chunkCoordinates.y / regionSizeInChunks) * mapSizeInRegions;
    }

    [BurstCompile]
    public int GetRegionChunkIndex(int2 chunkCoordinates)
    {
        return (int)(chunkCoordinates.x % regionSizeInChunks) + (int)(chunkCoordinates.y % regionSizeInChunks) * regionSizeInChunks;
    }
   
    [BurstCompile]
    public int GetRegionChunkIndex(int chunkIndex)
    {
        return GetRegionChunkIndex(GetChunkCoordinates(chunkIndex));
    }
   
   
    [BurstCompile]
    public int GetChunkIndexFromCoordinates(int2 coordinates)
    {
        return coordinates.x + coordinates.y * mapSizeInChunks;
    }
    [BurstCompile]
    public int GetChunkIndexFromEnginePosition(float2 enginePosition)
    {
        if(enginePosition.x >= 0 && enginePosition.y >= 0 && enginePosition.x < mapSizeInEngine &&  enginePosition.y < mapSizeInEngine)
            return (int)(enginePosition.x / tileSize / chunkSizeInTiles) 
                + (int)(enginePosition.y / tileSize / chunkSizeInTiles) * mapSizeInChunks;
        else
            return -1;
    }

    [BurstCompile]
    public float2 GetCorrectPosition(float2 enginePosition)
    {
        return new float2(math.clamp(enginePosition.x,0,mapSizeInEngine),math.clamp(enginePosition.y,0,mapSizeInEngine));
    }


    [BurstCompile]
    public void GetCorrectChunkAndPosition(float2 enginePosition,out int chunkIndex,out float2 correctPosition)
    {
        chunkIndex = GetChunkIndexFromEnginePosition(enginePosition);
        if(chunkIndex == -1)
        {
            correctPosition = GetCorrectPosition(enginePosition);
            chunkIndex = GetChunkIndexFromEnginePosition(correctPosition);
        }
        else
            correctPosition = enginePosition;
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