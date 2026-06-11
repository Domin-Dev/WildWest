
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
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

    /// <summary>
    /// it defines the maximum amount of unused space allowed in the file. 0.3 => 30% max unused
    /// </summary>
    public float defragmentationLimit;
}


public struct WorldItemsConfig: IComponentData
{
    public CollisionFilter FilterToFindSimilarWorldItems;
    public float2 sizeWorldItemCollider;
}