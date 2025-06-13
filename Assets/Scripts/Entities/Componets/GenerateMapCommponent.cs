using Unity.Entities;
using Unity.Mathematics;

public struct LoadMap : IComponentData { }
public struct GenerateMap : IComponentData { }
public struct SendMap  : IComponentData {
    public float2 position;
}