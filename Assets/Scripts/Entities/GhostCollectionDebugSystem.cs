using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class GhostCollectionDebugSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (SystemAPI.TryGetSingleton<GhostCollection>(out var collection))
        {
        }
    }
}
