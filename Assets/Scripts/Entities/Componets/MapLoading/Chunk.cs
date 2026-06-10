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
    [GhostField] public int chunkIndex;
    [GhostField] public float2 worldPos;
    [GhostField] public int regionIndex;
}

public struct ChunkTag : ICleanupComponentData
{
    public int value;

}

public struct ChunkTimestamp : IComponentData
{
    public double timestamp;
}


public struct ChunkObjects : IBufferElementData
{
    public Entity entity;
    public int ghostID;

    public ChunkObjects(Entity entity,int ghostID)
    {
        this.entity = entity;
        this.ghostID = ghostID;
    }
}

public struct PlayersNeedChunk : IBufferElementData
{
    public Entity playerEntity;
    public int networkID;
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
    [GhostField] public int2 globalTilePos;
    [GhostField] public int  id;
    [GhostField] public short variantIndex;
    [GhostField] public short stateIndex;

    [GhostField] public float hitPoints;
    [GhostField] public float maxHitPoints;
}

public struct NewChunk : IComponentData, IEnableableComponent{}

public struct ToSave : IComponentData, IEnableableComponent{}
