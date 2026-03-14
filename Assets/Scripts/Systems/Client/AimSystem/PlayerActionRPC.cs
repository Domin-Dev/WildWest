
using Unity.Mathematics;
using Unity.NetCode;

public struct PlayerActionRPC : IRpcCommand,ISetPlayer
{
    public int networkID;
    public float rotation;
    public float3 position;
    public NetworkTick tick;

    public void SetPlayer(int networkID,NetworkTick tick)
    {
        this.networkID = networkID;
        this.tick = tick;
    }
}

public interface ISetPlayer
{
    public void SetPlayer(int networkID,NetworkTick tick);
}