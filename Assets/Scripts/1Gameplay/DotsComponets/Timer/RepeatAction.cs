using Unity.Entities;
using Unity.NetCode;
public struct RepeatAction : IComponentData
{
    public NetworkTick Tick;
    public uint Interval;
    public RepeatAction(uint interval)
    {
        this.Tick = NetworkTick.Invalid;
        this.Interval = interval;
    }
}