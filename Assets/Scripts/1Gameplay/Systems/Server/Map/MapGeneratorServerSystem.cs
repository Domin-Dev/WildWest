using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;
using Unity.Collections;
using System;


[UpdateAfter(typeof(PlayerMoveSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class MapGeneratorServerSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<GenerateMap>();

    }
    protected override void OnUpdate()
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (generateMap, e) in SystemAPI.Query<RefRO<GenerateMap>>().WithEntityAccess())
        {
            ClientServerBootstrap.ServerWorld
                .GetExistingSystemManaged<ChunkManagementServerSystem>()
                .GenerateMap();
            entityCommandBuffer.DestroyEntity(e);
        }
        entityCommandBuffer.Playback(this.EntityManager);
        entityCommandBuffer.Dispose();
    }
    
}

