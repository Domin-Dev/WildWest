

using System;
using Unity.Collections;


public struct ContainerSave : IDisposable
{
    public int containerIndex;
    public int capacity;
    public NativeArray<SlotSave> slots;
    public NativeArray<BarDataSave> barData;

    public void Dispose()
    {
        if(slots.IsCreated) slots.Dispose();
        if(barData.IsCreated) barData.Dispose();
    }
}
