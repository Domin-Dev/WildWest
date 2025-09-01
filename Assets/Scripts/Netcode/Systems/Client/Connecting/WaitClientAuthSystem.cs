using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;
using System;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class WaitClientAuthSystem : SystemBase
{

    protected override void OnCreate()
    {
        RequireForUpdate<AuthResponse>();
    }
    protected override void OnUpdate()
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach ((RefRO < AuthResponse > response, RefRO<ReceiveRpcCommandRequest> rp, Entity e) in SystemAPI.Query<RefRO<AuthResponse>, RefRO<ReceiveRpcCommandRequest> >().WithEntityAccess())
        {
            if(response.ValueRO.success)
            {
                RPCHelper.SendRpc(ref entityCommandBuffer, new PlayerVerificationRPC() { playerName = SystemAPI.GetSingleton<PlayerName>().name });
                this.Enabled = false;
            }
            entityCommandBuffer.DestroyEntity(e);
        }


        entityCommandBuffer.Playback(this.EntityManager);
        entityCommandBuffer.Dispose();
    }
}

