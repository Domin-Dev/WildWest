
using Unity.Entities;
using Unity.NetCode;

public struct NewAmmoSelectedRPC : IRpcCommand, ISetPlayer
{
    public int networkID;
    public NetworkTick tick;
    public int ammoID;
    public int weaponID;

    public void SetPlayer(int networkID,NetworkTick tick)
    {
        this.networkID = networkID;
        this.tick = tick;
    }
}


public struct FutureReloadRPC : IComponentData,ISetPlayer
{
    public int networkID;
    public NetworkTick tick;
    public void SetPlayer(int networkID,NetworkTick tick)
    {
        this.networkID = networkID;
        this.tick = tick;
    }
}

