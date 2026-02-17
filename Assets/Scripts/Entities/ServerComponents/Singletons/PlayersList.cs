
using Unity.Collections;
using Unity.Entities;

public struct PlayersList: IBufferElementData
{
    public int networkID;
    public FixedString128Bytes playerName;
}