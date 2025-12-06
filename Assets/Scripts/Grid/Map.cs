using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System;
using Unity.Collections;

public class Map
{
    public Vector2 offset { private set;  get; }
    public float cellSize { private set;  get; }
    public int chunkSize { private set;  get; }
    public int widthInChunks { private set; get; }// in Chunks
    public int heightInChunks { private set; get; }// in Chunks

    public int chunkCount { private set; get; }
    /// <summary>
    /// in XY Cells
    /// </summary> 
    public int width { private set; get; }
    /// <summary>
    /// in XY Cells
    /// </summary> 
    public int height { private set; get; } 

    public float chunkSizeOnWorldScale { private set; get; }
    public float mapWidthOnWorldScale { private set; get; }
    public float mapHeightOnWorldScale { private set; get; }

    public Dictionary<int,Chunk> chunks;
   
    public Map(Vector2 offset, float cellSize, int chunkSize,int widthInChunks, int heightInChunks)
    {
        chunks = new Dictionary<int, Chunk>();
        this.offset = offset;
        this.cellSize = cellSize;  
        this.chunkSize = chunkSize;
        this.widthInChunks = widthInChunks;
        this.heightInChunks = heightInChunks;

        chunkCount = widthInChunks * heightInChunks;
        width = chunkSize * widthInChunks;
        height = chunkSize * heightInChunks;

        chunkSizeOnWorldScale = chunkSize * cellSize;
        mapHeightOnWorldScale = heightInChunks * chunkSizeOnWorldScale;
        mapWidthOnWorldScale = width * chunkSizeOnWorldScale;

    }

    public int GetChunkIndex(float2 position)
    {
        return ((int)(position.x / cellSize / chunkSize) + (int)(position.y / cellSize / chunkSize) * widthInChunks);
    }
    public int GetChunkIndex(float3 position)
    {
        return GetChunkIndex(MyTools.ConvertFloat(position));
    }
    public int2 GetChunkPos(int chunkIndex)
    {
        return new int2(chunkIndex % widthInChunks, chunkIndex / widthInChunks);
    }
    public List<int> GetNeighboringChunkIndexes(int chunkIndex,int renderSize)
    {
        List<int> indexes = new List<int>();
        int2 pos = GetChunkPos(chunkIndex);
        for (int y = -renderSize; y <= renderSize; y++)
        {
            for (int x = -renderSize; x <= renderSize; x++)
            {
                if (pos.x + x >= 0 && pos.y + y >= 0  && pos.x + x < widthInChunks && pos.y + y < heightInChunks)
                    indexes.Add(chunkIndex + x + y * widthInChunks);
            }
        }
        return indexes;   
    }
    public void GetNeighboringChunkIndexes(int chunkIndex, int renderSize, NativeHashSet<int> indexes)
    {
        int2 pos = GetChunkPos(chunkIndex);
        for (int y = -renderSize; y <= renderSize; y++)
        {
            for (int x = -renderSize; x <= renderSize; x++)
            {
                if (pos.x + x >= 0 && pos.y + y >= 0 && pos.x + x < widthInChunks && pos.y + y < heightInChunks)
                {
                    indexes.Add(chunkIndex + x + y * widthInChunks);
                }
            }
        }
    }
    public bool CheckChunkIndex(int chunkIndex)
    {
        return chunkIndex >= 0 && chunkIndex < widthInChunks * heightInChunks;
    }
    public float3 MapPositionToWorldPosition(int x, int y)
    {
        return new float3(offset.x + (x + 0.5f) * cellSize, offset.y + (y + 0.5f) * cellSize, offset.y + (y + 0.5f) * cellSize); 
    }
}

