using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(GhostSimulationSystemGroup))]
public partial struct PingUpdateClientSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {

        foreach (RefRO<NetworkSnapshotAck> ack in SystemAPI.Query<RefRO<NetworkSnapshotAck>>().WithAll<NetworkStreamInGame>())
        {
           // Debug.Log(ack.ValueRO.EstimatedRTT + "ms");
        }
    }
}
