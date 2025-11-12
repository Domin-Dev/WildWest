using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Entities;
using Unity.NetCode;


public static class EQHelperClient
{
       

    public static float CalculateMixPercentage(float volume1, float percent1, float volume2, float percent2)
    {
        return ((volume1 * percent1 + volume2 * percent2) / (volume1 + volume2));
    }

    public static byte CalculateMixPercentageByte(float volume1, float percent1, float volume2, float percent2)
    {
       return (byte) Math.Ceiling(CalculateMixPercentage(volume1,percent1, volume2, percent2));    
    }
}