using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.EventSystems.EventTrigger;


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
[UpdateAfter(typeof(NetworkReceiveSystemGroup))]
public partial struct ClientConnectionEventListener : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);
        var connectionEventsForClient = SystemAPI.GetSingleton<NetworkStreamDriver>().ConnectionEventsForTick;
        foreach (var evt in connectionEventsForClient)
        {
            switch (evt.State)
            {
                case ConnectionState.State.Disconnected:
                    if (!GameInfo.instance.isHost && GameInfo.instance.isInGame)
                    {
                        WindowsManager.instance.escScene = -1;
                        GameInfo.instance.errorMessage = "Lost connection to server.";
                        SceneManager.LoadScene(10);
                    }
                    GameInfo.instance.isInGame = false;
                    break; 
                case ConnectionState.State.Connected:
                    GameInfo.instance.isInGame = true;
                    break;
            }


            UnityEngine.Debug.Log($"[{state.WorldUnmanaged.Name}] {evt.ToFixedString()}!");
        }
        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}
