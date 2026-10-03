using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;
using System;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class IsConnetedCilientSystem : SystemBase
{

    public static event Action youAreInGame;


    bool isConnected = false;
    protected override void OnUpdate()
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        bool inGame = false;
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (YouAreInGameRPC, e) in SystemAPI.Query<RefRO<YouAreInGameRPC>>().WithEntityAccess())
        {
            entityCommandBuffer.DestroyEntity(e);
            isConnected = true;
        }

        foreach (var (player, e) in SystemAPI.Query<RefRO<Player>>().WithAll<GhostOwnerIsLocal>().WithEntityAccess())
        {
            if (isConnected)
            {
                inGame = true;
                youAreInGame?.Invoke();
            }
        }


        entityCommandBuffer.Playback(this.EntityManager);
        entityCommandBuffer.Dispose();
        if (inGame) Enabled = false;
    }
}

