using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(NetworkReceiveSystemGroup))]
[BurstCompile]
public partial struct NetCodeConnectionEventListener : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var connectionEventsForClient = SystemAPI.GetSingleton<NetworkStreamDriver>().ConnectionEventsForTick;
        foreach (var evt in connectionEventsForClient)
        {
            switch (evt.State)
            {
                case ConnectionState.State.Disconnected:
                    SaveSystem.Save();
                    Debug.Log("saved!");
                    break;
                case ConnectionState.State.Connecting:
                    break;
                case ConnectionState.State.Connected:
                    break;
            }

            UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] {evt.ToFixedString()}!");
        }
    }
}
