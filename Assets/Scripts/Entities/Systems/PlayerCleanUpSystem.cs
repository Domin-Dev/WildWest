using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;



public struct Players : IComponentData
{
    public NativeHashMap<int,Entity> hashMap;
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(NewPlayerSystem))]
partial struct PlayerCleanUpSystem : ISystem
{
    private NativeHashMap<int,Entity> players;
    public void OnCreate(ref SystemState state)
    {
        players = new NativeHashMap<int, Entity>(32,Allocator.Persistent);
        var entity = state.EntityManager.CreateEntity(typeof(Players));
        SystemAPI.SetComponent(entity,new Players()
        {
            hashMap = players
        });

        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<PlayerCleanUp>().WithNone<Player>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();

    }

    public void OnDestroy(ref SystemState state)
    {
        state.CompleteDependency();
        players.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        foreach ((RefRO<PlayerCleanUp> player, Entity entity) in SystemAPI.Query<RefRO<PlayerCleanUp>>().WithEntityAccess())
        {
            players.Remove(player.ValueRO.networkID);
            ecb.RemoveComponent<PlayerCleanUp>(entity);
        }
        ecb.Playback(state.EntityManager);
    }
}