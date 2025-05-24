using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Windows;

partial struct ParticleDestructionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NativeList<Entity> entitiesToDestroy = new NativeList<Entity>(40,Allocator.TempJob);
        ParticleDestructionJob job = new ParticleDestructionJob
        {
            elapsedTime = SystemAPI.Time.ElapsedTime,
            entitiesToDestroy = entitiesToDestroy.AsParallelWriter(),
        };

        JobHandle jobHandle = job.ScheduleParallel(state.Dependency);
        jobHandle.Complete();

        for (int i = 0; i < entitiesToDestroy.Length; i++)
        {
            if (entitiesToDestroy[i] != null) state.EntityManager.DestroyEntity(entitiesToDestroy[i]);
        }
        entitiesToDestroy.Dispose();
    }
}

[BurstCompile]
public partial struct ParticleDestructionJob : IJobEntity
{
    public double elapsedTime;
    public NativeList<Entity>.ParallelWriter entitiesToDestroy;

    public void Execute(ref SelfDestruction particles, Entity entity)
    {
        if (particles.finishParticles <= elapsedTime)
        {
            entitiesToDestroy.AddNoResize(entity);
        }
    }
}