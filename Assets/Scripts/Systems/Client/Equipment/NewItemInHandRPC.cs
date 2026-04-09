
using Unity.Entities;
using Unity.NetCode;

public struct NewItemInHandRPC : IRpcCommand, ISetPlayer
{
    public int networkID;
    public NetworkTick tick;

    public void SetPlayer(int networkID,NetworkTick tick)
    {
        this.networkID = networkID;
        this.tick = tick;
    }
}

public struct SendEventToPlayers : IBufferElementData
{
    public Entity connection;
}

public struct FutureEventsForPlayer : IBufferElementData
{
    public Entity entityEvent;
}

public struct ServerEventData  : IComponentData
{
    public NetworkTick tick;

    public ServerEventData(NetworkTick networkTick)
    {
        this.tick = networkTick;
    }
}

public struct WaitForProcess: IComponentData, IEnableableComponent {  }




