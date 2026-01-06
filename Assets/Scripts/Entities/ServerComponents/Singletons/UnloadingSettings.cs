
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[BurstCompile]
public struct UnloadingSettings: IComponentData
{
    /// <summary>
    /// chunk unloading period in seconds
    /// </summary>
    public int chunkUnloadingPeriod;

    /// <summary>
    /// number of loaded chunks per player
    /// </summary>
    public int loadedChunksPerPlayer;
    public int maxIdleChunkTime;
}