using System;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;

public struct EnableConnectionTimeoutCheck : IComponentData
{}


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]

public partial class ConnectionTimeoutSystem : SystemBase
{
    private double startTime;
    private const float timeoutSeconds = 2f;

    public static event Action connectionFailed;
    public static event Action connectionSuccessful;

    protected override void OnCreate()
    {
        RequireForUpdate<EnableConnectionTimeoutCheck>();
        base.OnCreate();
    }

    protected override void OnStartRunning()
    {
        startTime = SystemAPI.Time.ElapsedTime;
    }

    protected override void OnUpdate()
    {
        bool isConnected = false;
        Entities.WithAll<NetworkStreamConnection>().ForEach((NetworkStreamConnection n, Entity e) =>
        {
            if (n.CurrentState == ConnectionState.State.Connected)
            {
                isConnected = true;
                return;
            }
        }).WithoutBurst().Run();

        if(isConnected)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            Connected(ref ecb);
            ecb.Playback(ClientServerBootstrap.ClientWorld.EntityManager);
            ecb.Dispose();
            return;
        }

        if (SystemAPI.Time.ElapsedTime - startTime > timeoutSeconds)
        {
            Debug.Log("Nie uda³o siê po³¹czyæ z serwerem (timeout)!");
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            Entities.WithImmediatePlayback().ForEach((Entity entity, in NetworkStreamConnection req) =>
            {
                ecb.RemoveComponent<NetworkStreamConnection>(entity);
            }).Run();

            ecb.Playback(ClientServerBootstrap.ClientWorld.EntityManager);
            ecb.Dispose();
            connectionFailed?.Invoke();
            Enabled = false;
        }
    }
    private void Connected(ref EntityCommandBuffer entityCommandBuffer)
    {
        Debug.Log("? Po³¹czenie nawi¹zane!");
        connectionSuccessful?.Invoke();
        entityCommandBuffer.DestroyEntity(SystemAPI.GetSingletonEntity<EnableConnectionTimeoutCheck>());
        RPCHelper.SendRpc(ref entityCommandBuffer, new PlayerVerificationRPC() { playerName = SystemAPI.GetSingleton<PlayerName>().name });
        this.Enabled = false;
    }
}
