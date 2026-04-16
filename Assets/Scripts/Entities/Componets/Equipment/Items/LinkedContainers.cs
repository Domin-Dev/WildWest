using System.Runtime.InteropServices;
using Unity.Entities;
using Unity.NetCode;

[GhostComponent] 
public struct LinkedContainers : IBufferElementData, IGetSlot
{
    [GhostField] public int slot;
    [GhostField] public int containerIndex;
    [GhostField] public Entity containerEntity;


    public int GetSlot()
    {
        return slot;
    }

    public void SetSlot(int slot)
    {
        this.slot = slot;
    }
}


