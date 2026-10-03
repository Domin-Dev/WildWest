
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;


[UpdateAfter(typeof(TransformSystemGroup))]
partial struct FollowersSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TransformFollower>();
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(state.WorldUpdateAllocator);
        foreach ((DynamicBuffer<TransformFollower> followers,RefRO<LocalToWorld> position,Entity entity) in SystemAPI.Query<DynamicBuffer<TransformFollower>,RefRO<LocalToWorld>>().WithEntityAccess())
        {
            foreach(var follower in followers)
            {
                float3 newPosition = position.ValueRO.Position;
                if(!follower.followPositionZ)
                    newPosition.z = follower.follower.Value.position.z;
                follower.follower.Value.position = newPosition + follower.offset;   
            }
        }
        entityCommandBuffer.Playback(state.EntityManager);
    }
}



