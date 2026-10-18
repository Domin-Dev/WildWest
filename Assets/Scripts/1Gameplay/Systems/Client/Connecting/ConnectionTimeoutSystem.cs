using System;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct EnableConnectionTimeoutCheck : IComponentData
{
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
public partial class ConnectionTimeoutSystem : SystemBase
{
    private double startTime;
    private const double TimeoutSeconds = 20.0;

    public static event Action connectionFailed;
    public static event Action connectionSuccessful;

    protected override void OnCreate()
    {
        RequireForUpdate<EnableConnectionTimeoutCheck>();
    }

    protected override void OnStartRunning()
    {
        startTime = SystemAPI.Time.ElapsedTime;
    }

    protected override void OnUpdate()
    {
        bool isConnected = false;

        foreach (var connection in
                 SystemAPI.Query<RefRO<NetworkStreamConnection>>())
        {
            if (connection.ValueRO.CurrentState ==  ConnectionState.State.Connected)
            {
                isConnected = true;
                break;
            }
        }

        if (isConnected)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            Entity marker = SystemAPI.GetSingletonEntity<EnableConnectionTimeoutCheck>();

            ecb.DestroyEntity(marker);
            ecb.Playback(EntityManager);
            ecb.Dispose();

            connectionSuccessful?.Invoke();
            Enabled = false;
            return;
        }

        if (SystemAPI.Time.ElapsedTime - startTime > TimeoutSeconds)
        {
            Debug.LogWarning(
                "Nie udało się połączyć z serwerem (timeout)!");

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (_, entity) in SystemAPI.Query<RefRO<NetworkStreamConnection>>().WithEntityAccess())
            {
                ecb.RemoveComponent<NetworkStreamConnection>(entity);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
            connectionFailed?.Invoke();
            Enabled = false;
        }
    }
}
