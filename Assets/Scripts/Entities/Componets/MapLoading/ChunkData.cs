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

public struct ChunkObjects : IBufferElementData
{
    public Entity entity;
}



[GhostComponent(OwnerSendType = SendToOwnerType.All)]
public struct ChunkTiles : IBufferElementData
{
    [GhostField] public int tileID;
    [GhostField] public byte variant;
}

[GhostComponent(OwnerSendType = SendToOwnerType.All)]
public struct BuildingObjects : IBufferElementData
{
    [GhostField] public int2 position;
    [GhostField] public int id;
    [GhostField] public short variantIndex;
    [GhostField] public short stateIndex;

    [GhostField] public float hitPoints;
    [GhostField] public float maxHitPoints;
}

public struct NewChunk : IComponentData, IEnableableComponent{}


