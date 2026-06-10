
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Unity.NetCode
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(GhostSimulationSystemGroup))]
    [UpdateBefore(typeof(GhostDespawnSystem))]



    partial struct ChunkRespawnClientSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MoveToTarget>();
        }
        public void OnUpdate(ref SystemState state)
        {
          //  SystemAPI.GetSingletonRW<GhostDespawnQueues>();
        }
    } 
}