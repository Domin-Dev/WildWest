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
     public byte action;
}
// Action
// 0 - start streaming chunk
// 1 - stop streaming chunk


[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
public struct ChunkEvents : IBufferElementData ,IIndexed
{
    [GhostField] public int2 value;
    [GhostField] public byte flags;
    [GhostField] public uint index;

    public uint GetIndex()
    {
        return index;
    }
    // byte
    // 0 null
    // 1 readChunk Value.x = chunk index
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