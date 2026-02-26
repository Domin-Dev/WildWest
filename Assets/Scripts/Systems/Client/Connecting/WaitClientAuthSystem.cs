using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        foreach ((RefRO <AuthResponse> response, RefRO<ReceiveRpcCommandRequest> rp, Entity e) in SystemAPI.Query<RefRO<AuthResponse>, RefRO<ReceiveRpcCommandRequest> >().WithEntityAccess())
        {
            if(response.ValueRO.success)
            {
                this.Enabled = false;
            }
            else
            {
                GameInfo.instance.errorMessage = "Incorrect password.";
                SceneManager.LoadScene(10);
            }
            entityCommandBuffer.DestroyEntity(e);
        }


        entityCommandBuffer.Playback(this.EntityManager);
        entityCommandBuffer.Dispose();
    }
}

