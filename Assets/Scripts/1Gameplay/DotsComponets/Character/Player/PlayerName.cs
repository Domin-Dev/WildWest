using Unity.Collections;
using Unity.Entities;

public struct PlayerName : IComponentData
{
    public FixedString128Bytes name;
}