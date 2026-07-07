using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;


public struct NewChunkServerAction : IComponentData, IEnableableComponent{}
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

public enum ServerAction : byte
{
    StartStreamingChunk = 0,
    StopStreamingChunk = 1,
    DamageBuildingObject = 2
}





[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
public struct ChunkEvents : IBufferElementData ,IIndexed
{
    [GhostField] public int chunk;
    [GhostField] public int2 tilePosition;
    [GhostField] public ChunkEventType flags;
    [GhostField] public uint index;

    public uint GetIndex()
    {
        return index;
    }
    // byte
    // 0 null
    // 1 readChunk Value.x = chunk index
}

public enum ChunkEventType : byte
{
    LoadChunk = 1,
    UnloadChunk = 2,
    UpdateBuildingObject = 3
}




[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct ChunkEventCounter : IInputComponentData
{
    [GhostField(Quantization = 0)] public uint index;
}
public struct ServerChunkEventCounter : IComponentData,IIndexed
{
    public uint index;
    public uint GetIndex() { return index; }
}