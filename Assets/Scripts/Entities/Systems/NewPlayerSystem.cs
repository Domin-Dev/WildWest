using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;


[UpdateInGroup(typeof(SimulationSystemGroup),OrderFirst = true)]
partial struct NewPlayerSystem : ISystem
{

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NewPlayerTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach ((RefRO<Player> player, Entity entity) in SystemAPI.Query<RefRO<Player>>().WithAll<NewPlayerTag>().WithEntityAccess())
        {
            var childs = SystemAPI.GetBuffer<Child>(entity);
            foreach (var item in childs)
            {
                if (state.EntityManager.HasComponent(item.Value, typeof(TextMesh)))
                {
                    var textMesh = state.EntityManager.GetComponentObject<TextMesh>(item.Value);
                    textMesh.text = player.ValueRO.playerName.ToString();


                    Debug.Log("dziala!");
                }
            }


            entityCommandBuffer.RemoveComponent<NewPlayerTag>(entity);
        }


        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }
}