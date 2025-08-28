using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;


public struct SendChunk : IComponentData, IEnableableComponent{}
public struct ChunkRecipients : IBufferElementData
{
     public int networkID;
}


[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)]
public struct ChunkEvents : IBufferElementData
{
    [GhostField] public int2 value;
    [GhostField] public byte flags;
    [GhostField] public uint index;
    // byte
    // 0 null
    // 1 readChunk Value.x = chunk index
}

public struct ChunkEventCounter : IComponentData
{
    public uint index;
}