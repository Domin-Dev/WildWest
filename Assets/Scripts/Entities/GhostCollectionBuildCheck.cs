using Unity.Entities;
using Unity.NetCode;
using UnityEngine;


[UpdateInGroup(typeof(SimulationSystemGroup))]

public partial class GhostCollectionBuildCheck : SystemBase
{
    protected override void OnUpdate()
    {
        if (!SystemAPI.HasSingleton<GhostCollection>())
        {
            Debug.LogError("GhostCollection not found!");
            return;
        }

        var ghostEntity = SystemAPI.GetSingletonEntity<GhostCollection>();
        var buffer = EntityManager.GetBuffer<GhostCollectionPrefab>(ghostEntity);
        Debug.Log("Ghost prefabs in build: " + buffer.Length);

        foreach (var ghost in buffer)
        {
            var name = EntityManager.GetName(ghost.GhostPrefab);
            Debug.Log($"Ghost Entity: {ghost.GhostPrefab} - Name: {name}");
        }

        Enabled = false;
    }
}