
using Unity.Entities;

public struct LoadedChunks: IBufferElementData
{
    public Entity chunkEntity; 
    public int chunkIndex;
}