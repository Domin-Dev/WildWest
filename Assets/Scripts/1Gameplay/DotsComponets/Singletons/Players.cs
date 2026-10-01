using Unity.Collections;
using Unity.Entities;

public struct Players : IComponentData
{
    public NativeHashMap<int,Entity> hashMap;
}