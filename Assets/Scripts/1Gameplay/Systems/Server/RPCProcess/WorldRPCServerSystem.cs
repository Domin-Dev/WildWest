using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;



[UpdateInGroup(typeof(LateSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(RPCProcessingSystem))]
partial struct WorldRPCServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<LoadedChunks>();
        state.RequireForUpdate<Chunks>();
    }
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((RefRO<LightningRPC> rpc,DynamicBuffer<SendEventToPlayers> toPlayers, Entity entity) in
        SystemAPI.Query<RefRO<LightningRPC>,DynamicBuffer<SendEventToPlayers>>().WithNone<WaitForProcess>().WithEntityAccess())
        {
            for(int i = 0; i < toPlayers.Length;i++)
                RPCHelper.SendRpc(ecb,toPlayers[i].connection,in rpc.ValueRO);
            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);  
        ecb.Dispose();
    }
}