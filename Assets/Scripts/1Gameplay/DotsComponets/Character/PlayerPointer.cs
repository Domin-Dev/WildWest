using Unity.Entities;
using Unity.Mathematics;

public struct PlayerPointer : IComponentData
{
    public int2 LastGamePointerPosition;
    public bool updateTileInfo;
}