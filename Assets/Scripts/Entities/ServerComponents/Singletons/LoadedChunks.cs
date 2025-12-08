
using Unity.Entities;

public struct LoadedChunks: IBufferElementData
{
    public Entity entity; 
    public int index;
    public double time;
}