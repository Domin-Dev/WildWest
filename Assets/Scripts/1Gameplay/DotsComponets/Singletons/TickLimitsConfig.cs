
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[BurstCompile]
public struct TickLimitsConfig: IComponentData
{
    public int maxLoadedChunksInTick;
    public int loadedChunksInTickPerClient;

    public int maxStopRequestsInTick;
    public int stopRequestsInTickPerClient;

    public int maxStartRequestsInTick;
    public int startRequestsInTickPerClient;

    public int maxUpdateChunkTimeRequestsInTick;
    public int updateChunkTimeRequestsInTickPerClient; 

    public int maxUnloadedChunksInTick;
    public int unloadedChunksInTickPerClient;
    
}