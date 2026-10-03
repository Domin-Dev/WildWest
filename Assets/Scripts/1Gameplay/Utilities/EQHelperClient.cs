using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Entities;
using Unity.NetCode;



public enum SelectionMode

{
    N,
    All,
    Half,
}
public enum ContainerType
{
    Standard,
    Outfit
}

public static class EQHelperClient
{
    public static readonly (ContainerType type, int min, int maxExclusive)[] containerTypeRanges =
    {
        (ContainerType.Outfit, 10000,20000),
    };


    public static float CalculateMixPercentage(float volume1, float percent1, float volume2, float percent2)
    {
        return ((volume1 * percent1 + volume2 * percent2) / (volume1 + volume2));
    }

    public static int ConvetSlotIndexToSelectedSlotIndex(int slotIndex)
    {
        return -(slotIndex + 1);
    }


    // normal == no selected
    public static int GetNormalSlotIndex(int slotIndex)
    {
        if(slotIndex < 0) return ConvetSlotIndexToSelectedSlotIndex(slotIndex);
        return slotIndex;
    }


    public static ContainerType GetContainerType(int containerIndex)
    {
        ContainerType type = ContainerType.Standard;
        foreach (var item in containerTypeRanges)
        {
            if(containerIndex >= item.min && containerIndex < item.maxExclusive)
            {
                type = item.type;
            }
        }
        return type;
    }
}