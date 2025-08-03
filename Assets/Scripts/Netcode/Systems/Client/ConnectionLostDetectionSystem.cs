using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.SceneManagement;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct ConnectionLostDetectionSystem : ISystem
{

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
    }

    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<NetworkStreamConnection>())
        {
            GameInfo.instance.errorMessage = "Lost connection to server.";
            SceneManager.LoadScene(10);
        }
    }
}
