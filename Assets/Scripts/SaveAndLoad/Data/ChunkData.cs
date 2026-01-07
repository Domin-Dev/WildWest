

using System;
using Unity.Collections;


public struct ChunkData : IDisposable
{
    public int localChunkIndex;
    public int chunkIndex;
    public NativeArray<TileData> tiles;
    public NativeArray<BuildingObjectData> objects;

    public void Dispose()
    {
        tiles.Dispose();
        objects.Dispose();
    }
}
