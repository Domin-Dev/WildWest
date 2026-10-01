using Unity.Collections;
using Unity.Entities;

public struct ClientSalt : IComponentData
{
    public FixedString128Bytes salt;
}