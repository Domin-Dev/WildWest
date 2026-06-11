
using NUnit.Framework.Constraints;
using Unity.Entities;
using Unity.Mathematics;
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

public struct DropItemRPC : IRpcCommand, ISetPlayer
{
    public int networkID;
    public NetworkTick tick;
    public int chunkIndex;
    public int slotIndex;
    public float2 dropPosition;
    public float duration;

    public DropItemRPC(int chunkIndex, int slotIndex, float2 dropPosition, float duration)
    {
        this.chunkIndex = chunkIndex;
        this.slotIndex = slotIndex;
        this.dropPosition = dropPosition;
        this.networkID = 0;
        this.duration = duration;
        this.tick = NetworkTick.Invalid;
    }

    public void SetPlayer(int networkID,NetworkTick tick)
    {
        this.networkID = networkID;
        this.tick = tick;
    }
}


public struct PickUpItemRPC : IRpcCommand, ISetPlayer
{
    public int networkID;
    public NetworkTick tick;
    public int chunkIndex;
    public int slotIndex;
    public float duration;
    public bool destroyItem;
    public PickUpItemRPC(int chunkIndex, int slotIndex, float duration, bool destroyItem)
    {
        this.chunkIndex = chunkIndex;
        this.slotIndex = slotIndex;
        this.networkID = 0;
        this.duration = duration;
        this.tick = NetworkTick.Invalid;
        this.destroyItem = destroyItem;
    }

    public void SetPlayer(int networkID,NetworkTick tick)
    {
        this.networkID = networkID;
        this.tick = tick;
    }
}

public struct MergeItemsPRC : IRpcCommand, ISetPlayer
{
    public int networkID;
    public NetworkTick tick;

    public int fromChunkIndex;
    public int fromSlotIndex;

    public int toChunkIndex;
    public int toSlotIndex;


    public float duration;



    public void SetPlayer(int networkID,NetworkTick tick)
    {
        this.networkID = networkID;
        this.tick = tick;
    }
}



public struct SpawnWorldItem : IComponentData
{
    public float2 dropPosition;
    public int slotIndex;
    public int chunkIndex;
    public  InventorySlot item;
    public Entity chunk;
}


public struct PickUpItemCompleted : IComponentData, ISetPlayer
{
    public int networkID;
    public NetworkTick tick;
    public int chunkIndex;
    public int slotIndex;
 
    public void SetPlayer(int networkID,NetworkTick tick)
    {
        this.networkID = networkID;
        this.tick = tick;
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


public struct CreateWorldItemRPC : IRpcCommand,ISetPlayer
{
    public int networkID;
    public NetworkTick tick;
    public int chunkIndex;
    public int slotIndex;


    public void SetPlayer(int networkID,NetworkTick tick)
    {
        this.networkID = networkID;
        this.tick = tick;
    }
}