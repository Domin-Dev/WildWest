
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

public struct ReloadRPC : IRpcCommand, ISetPlayer
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

    public NewAmmoSelectedRPC GetRPC()
    {
        return new NewAmmoSelectedRPC()
        {
            networkID = this.networkID,
            tick = this.tick,
            ammoID = this.ammoID,
            weaponID = this.weaponID
        };
    }
}


public struct UnloadRPC : IRpcCommand, ISetPlayer
{
    public int networkID;
    public NetworkTick tick;
    public int weaponID;
    public int ammoID;

    public void SetPlayer(int networkID,NetworkTick tick)
    {
        this.networkID = networkID;
        this.tick = tick;
    }
}



public struct StopReloadRPC : IRpcCommand,ISetPlayer
{
    public int networkID;
    public NetworkTick tick;
    public int weaponID;
    
    public void SetPlayer(int networkID,NetworkTick tick)
    {
        this.networkID = networkID;
        this.tick = tick;
    }
}

public struct EndReload : IComponentData,ISetPlayer
{
    public int networkID;
    public NetworkTick tick;
    public int ammoID;
    
    public void SetPlayer(int networkID,NetworkTick tick)
    {
        this.networkID = networkID;
        this.tick = tick;
    }
}
public struct EndUnload : IComponentData,ISetPlayer
{
    public int networkID;
    public NetworkTick tick;
    
    public void SetPlayer(int networkID,NetworkTick tick)
    {
        this.networkID = networkID;
        this.tick = tick;
    }
}
public struct FutureReload : IComponentData,ISetPlayer
{
    public int networkID;
    public NetworkTick tick;
    public void SetPlayer(int networkID,NetworkTick tick)
    {
        this.networkID = networkID;
        this.tick = tick;
    }
}

