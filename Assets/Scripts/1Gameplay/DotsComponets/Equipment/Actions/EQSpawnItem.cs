using Unity.Entities;
using Unity.Mathematics;

public struct EQSpawnItem : IComponentData
{
    public InventorySlot item;
    public float barValue;
    public int chunkIndex;
    public float2 position;
    public float2 fromPosition;
    public float duration;
}