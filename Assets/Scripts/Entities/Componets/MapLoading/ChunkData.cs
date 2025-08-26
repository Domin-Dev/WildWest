using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;

[GhostComponent(OwnerSendType = SendToOwnerType.All)]
public struct ChunkData : IComponentData
{
    [GhostField] public int Value;
    [GhostField] public int Max;
}
public struct SentChunks: IBufferElementData
{
    public int chunkIndex;
}

public struct NeedChunks : IComponentData, IEnableableComponent{}

