using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct TransformFollower : ICleanupBufferElementData
{
    public UnityObjectRef<Transform> follower;
    public float3 offset;
    public bool followPositionZ;

    public TransformFollower(Transform transform,float3 offset = default,bool followPositionZ = true)
    {
        this.follower = transform;
        this.offset = offset;
        this.followPositionZ = followPositionZ;
    }
}

