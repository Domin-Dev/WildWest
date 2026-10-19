using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public struct RPCCleanUp : IComponentData 
{
    public uint CleanupPeriod;
    public uint MaxRpcAgeFrames;
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(SimulationSystemGroup))]
partial struct RPCCleanUpSystem : ISystem
{ 
    EntityQuery cleanUpQuery;
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<RPCCleanUp,Execute>();

        EntityQueryBuilder entityQueryBuilder2 = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ReceiveRpcCommandRequest>();

        cleanUpQuery = state.GetEntityQuery(entityQueryBuilder);
        state.RequireForUpdate(cleanUpQuery);
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder2));

        entityQueryBuilder.Dispose();
        entityQueryBuilder2.Dispose();
    }
    public void OnUpdate(ref SystemState state)
    {   
        if(!cleanUpQuery.IsEmpty)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            var cleanUp = SystemAPI.GetSingleton<RPCCleanUp>();
            var cleanUpEntity = SystemAPI.GetSingletonEntity<RPCCleanUp>();
            
            foreach ((RefRO<ReceiveRpcCommandRequest> request, Entity entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                if(request.ValueRO.Age > cleanUp.MaxRpcAgeFrames)
                {
                    #if UNITY_EDITOR
                        UnityEngine.Debug.Log($"RPC ({entity.ToString()}) has been deleted");
                    #endif
                    ecb.DestroyEntity(entity);
                }
            }

            ecb.RemoveComponent<Execute>(cleanUpEntity);
            ecb.Playback(state.EntityManager);
        }
    }
}