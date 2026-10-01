
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[GhostComponent(OwnerSendType = SendToOwnerType.All)]
public struct WorldItemPosition : IBufferElementData,IGetSlot
{
    [GhostField] public float2 worldItemPos;
    [GhostField] public int slot;

    public void SetSlot(int slot)
    {
        this.slot = slot;
    }
    public int GetSlot() { return slot; }
}