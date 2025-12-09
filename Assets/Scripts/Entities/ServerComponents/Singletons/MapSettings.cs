
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct MapSettings: IComponentData
{
    public int seed;
    public int widthInChunks;
    public int heightInChunks;




    // Server fields
    public int playerRenderSize; 
    public int maxChunksPerClient;

    public int maxLoadedChunksInTick;
    public int loadedChunksInTickPerClient;


    



    public int playerRenderCount => (2 * playerRenderSize + 1)*(2 * playerRenderSize + 1);




    [BurstCompile]
    public void GetNeighboringChunkIndexes(int chunkIndex,NativeHashSet<int> indexes)
    {
        int2 pos = GetChunkPos(chunkIndex);
        for (int y = -playerRenderSize; y <= playerRenderSize; y++)
        {
            for (int x = -playerRenderSize; x <= playerRenderSize; x++)
            {
                if (pos.x + x >= 0 && pos.y + y >= 0 && pos.x + x < widthInChunks && pos.y + y < heightInChunks)
                {
                    indexes.Add(chunkIndex + x + y * widthInChunks);
                }
            }
        }
    }

    [BurstCompile]
    public int2 GetChunkPos(int chunkIndex)
    {
        return new int2(chunkIndex % widthInChunks, chunkIndex / widthInChunks);
    }
    
    [BurstCompile]
    public bool CheckChunkIndex(int chunkIndex)
    {
        return chunkIndex >= 0 && chunkIndex < widthInChunks * heightInChunks;
    }
}