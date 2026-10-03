

using System;
using Unity.Collections;


public struct ChunkSave : IDisposable
{
    public int localChunkIndex;
    public int chunkIndex;
    public NativeArray<TileSave> tiles;
    public NativeArray<BuildingObjectSave> objects;

    public void Dispose()
    {
        if(tiles.IsCreated) tiles.Dispose();
        if(objects.IsCreated) objects.Dispose();
    }
}
