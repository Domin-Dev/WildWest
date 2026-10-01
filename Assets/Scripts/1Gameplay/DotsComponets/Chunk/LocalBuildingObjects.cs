using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;


public struct LocalBuildingObjects : IBufferElementData, IGetGlobalTilePosition
{
    public int2 GlobalTilePosition => globalTilePos;
    public Entity localSpriteEntity;
    public Entity localEntity;
    public int2 globalTilePos;
    public int id;
}


