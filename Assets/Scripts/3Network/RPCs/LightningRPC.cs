using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

public struct LightningRPC : IRpcCommand
{
    public float2 position;
}

public struct Rain : IComponentData
{
    public float intensity; // 0 - 1
    public float time;
    public NetworkTick tick;
}

