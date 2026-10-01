
using Unity.Entities;
using Unity.Mathematics;

public struct ChunkServerActions : IBufferElementData
{
    public int networkID;
    public int2 tilePosition;
    public int value;
    public ServerAction action;
}
// Action
// 0 - start streaming chunk
// 1 - stop streaming chunk
// 2 - Damage building object