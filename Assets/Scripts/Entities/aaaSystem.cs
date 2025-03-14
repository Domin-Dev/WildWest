using TMPro;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial struct aaaSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        foreach ((RefRO<aa> localTransform, 
            DynamicBuffer<Child> CharacterAim
             ) in SystemAPI.Query<RefRO<aa>, DynamicBuffer<Child>>())
        {

            //Debug.Log(CharacterAim.Length.ToString());
          
        }
    }

}

