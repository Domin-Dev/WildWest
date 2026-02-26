using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[UpdateAfter(typeof(VariableSynchronizationServerSystem))]
[UpdateAfter(typeof(CollisionSystem))]

 partial struct CharacterHandsSystem : ISystem
{
    private static float leftSide = math.PI / 2f;

    private float deltaTime;
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();  
    }


  
}


