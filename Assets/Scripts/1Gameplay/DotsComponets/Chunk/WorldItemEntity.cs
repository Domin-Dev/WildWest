
using Unity.Entities;

public struct WorldItemEntity : ICleanupBufferElementData, IGetSlot
{
    public int slot;
    public Entity worldItem;

    public int GetSlot()
    {
        return slot;
    }

    public void SetSlot(int slot)
    {
        this.slot = slot;
    }
}
