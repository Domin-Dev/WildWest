
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[BurstCompile]
public struct SavesConfig: IComponentData
{
    /// <summary>
    /// period in seconds
    /// </summary>
    public int savePeriod;
    public int maxSavedChunksInTick;
    public int savedChunksInTickPerClient;
}