using Unity.Collections;
using Unity.Entities;

public struct PlayerName : IComponentData
{
    public FixedString64Bytes name;
}

public struct NewPlayerTag : IComponentData
{

}
