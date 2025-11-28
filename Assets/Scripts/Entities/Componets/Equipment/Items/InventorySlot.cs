using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[GhostComponent(OwnerSendType = SendToOwnerType.SendToOwner)] 
public struct InventorySlot : IBufferElementData, IGetSlot
{
    [GhostField] public int slot;
    [GhostField] public int itemId;    
    [GhostField] public int quantity;
    [GhostField] public float wetness; // 0% - 100%
    [GhostField] public float3 color; // 0% - 100%
    [GhostField] public Quality quality; 


    public override string ToString()
    {
        return $"Slot:{slot} ItemID:{itemId} Quantity:{quantity}";
    }

    public void UpdateWetness(float newValue)
    {
        wetness = math.clamp(newValue + wetness, 0.0f, 100.0f);
    }

    public int GetSlot() { return slot; }
}

public enum Quality : byte
{
    none = 0,
    terrible = 1,
    poor = 2,
    normal = 3,
    good = 4,
    masterful = 5,
    legendary = 6,
}



public struct ContainerLoaded : IComponentData
{
}


