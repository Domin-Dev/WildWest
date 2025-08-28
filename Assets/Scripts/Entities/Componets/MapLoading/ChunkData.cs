using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[GhostComponent(OwnerSendType = SendToOwnerType.All)]
public struct ChunkComponent : IComponentData
{
    [GhostField] public int index;
    [GhostField] public float2 worldPos;
}

[GhostComponent(OwnerSendType = SendToOwnerType.All)]
public struct ChunkTiles : IBufferElementData
{
    [GhostField] public int tileID;
    [GhostField] public byte variant;
}

public struct NeedChunks : IComponentData, IEnableableComponent{}


