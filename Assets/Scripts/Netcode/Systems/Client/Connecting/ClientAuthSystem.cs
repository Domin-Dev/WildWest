using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;
using System;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class ClientAuthSystem : SystemBase
{

    public static event Action<FixedString128Bytes> passwordRequired;
    protected override void OnCreate()
    {
        RequireForUpdate<PlayerSaltRPC>();
    }
    protected override void OnUpdate()
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach ((RefRO < PlayerSaltRPC > salt, RefRO<ReceiveRpcCommandRequest> rp, Entity e) in SystemAPI.Query<RefRO<PlayerSaltRPC>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
        {
            passwordRequired?.Invoke(salt.ValueRO.salt);
            entityCommandBuffer.DestroyEntity(e);
            this.Enabled = false;
        }


        entityCommandBuffer.Playback(this.EntityManager);
        entityCommandBuffer.Dispose();
    }
}

