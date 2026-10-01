
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[GhostComponent(OwnerSendType = SendToOwnerType.All)]
public struct BuildingObjects : IBufferElementData,IGetGlobalTilePosition
{
    public int2 GlobalTilePosition => globalTilePos;
    [GhostField(SendData = false)] public Entity localEntity;
    [GhostField] public int2 globalTilePos;
    [GhostField] public int  id;
    [GhostField] public short variantIndex;
    [GhostField] public short stateIndex;

    [GhostField] public int hitPoints;
    [GhostField] public int maxHitPoints;
}
