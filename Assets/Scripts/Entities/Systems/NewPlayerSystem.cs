using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(SimulationSystemGroup),OrderFirst = true)]
partial struct NewPlayerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NewPlayerTag>();
    }


    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach ((RefRO<Player> player, Entity entity) in SystemAPI.Query<RefRO<Player>>().WithAll<NewPlayerTag>().WithEntityAccess())
        {
            if (!SystemAPI.HasBuffer<Child>(entity)) continue;
               
            if (state.World.Flags == WorldFlags.GameServer && ClientServerBootstrap.HasClientWorlds)
            {
                entityCommandBuffer.AddComponent<DisableRendering>(entity);
                SetName(entity, ref state,string.Empty);
                continue;
            }

            SetName(entity,ref state, player.ValueRO.playerName.ToString());
            entityCommandBuffer.RemoveComponent<NewPlayerTag>(entity);
        }


        entityCommandBuffer.Playback(state.EntityManager);
        entityCommandBuffer.Dispose();
    }

    private void SetName(Entity entity, ref SystemState state, string name)
    {
        var childs = SystemAPI.GetBuffer<Child>(entity);

        foreach (var item in childs)
        {
            if (state.EntityManager.HasComponent(item.Value, typeof(TextMesh)))
            {
                var textMesh = state.EntityManager.GetComponentObject<TextMesh>(item.Value);
                textMesh.text = name;
            }
        }
    }
}