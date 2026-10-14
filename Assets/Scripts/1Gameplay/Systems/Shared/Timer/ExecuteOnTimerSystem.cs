 using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(SimulationSystemGroup),OrderFirst = true)]
public partial struct ExecuteOnTimerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();

        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ExecuteAtTick>().WithNone<Execute>();
        EntityQueryBuilder entityQueryBuilder2 = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<RepeatAction>().WithNone<Execute>();

        state.RequireAnyForUpdate(state.GetEntityQuery(entityQueryBuilder2),state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
        entityQueryBuilder2.Dispose();
    }
    public void OnUpdate(ref SystemState state)
    {
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        if(!currentTick.IsValid) return;
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        EntityCommandBuffer ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);


        foreach (var (executeAtTick, entity) in SystemAPI.Query<RefRO<ExecuteAtTick>>().WithNone<Execute>().WithEntityAccess())
        {
            if(executeAtTick.ValueRO.tick.IsValid && !executeAtTick.ValueRO.tick.IsNewerThan(currentTick))
                ecb.AddComponent<Execute>(entity);
        }

        foreach (var (repeatAction, entity) in SystemAPI.Query<RefRW<RepeatAction>>().WithNone<Execute>().WithEntityAccess())
        {
            if(!repeatAction.ValueRW.Tick.IsValid)
            {
                repeatAction.ValueRW.Tick = EntityHelper.AddTime(currentTick,repeatAction.ValueRO.Interval);
                continue;
            }

            if(!repeatAction.ValueRW.Tick.IsNewerThan(currentTick))
            {
                ecb.AddComponent<Execute>(entity);
                repeatAction.ValueRW.Tick = EntityHelper.AddTime(repeatAction.ValueRW.Tick,repeatAction.ValueRO.Interval);
            }
        }
    }
}

