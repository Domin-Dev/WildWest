using UnityEngine;
using System.Collections.Generic;

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
}

