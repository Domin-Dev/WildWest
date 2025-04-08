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
        NativeList<Entity> entitiesToDestroy = new NativeList<Entity>(Allocator.TempJob);

        ParticleDestructionJob job = new ParticleDestructionJob
        {
            elapsedTime = SystemAPI.Time.ElapsedTime,
            entitiesToDestroy = entitiesToDestroy,
        };
        JobHandle jobHandle = job.ScheduleParallel(state.Dependency);
        jobHandle.Complete();

        foreach (var item in entitiesToDestroy)
        {
            state.EntityManager.DestroyEntity(item);
        }


        entitiesToDestroy.Dispose();
    }
}


[BurstCompile]
public partial struct ParticleDestructionJob : IJobEntity
{
    public double elapsedTime;
    public NativeList<Entity> entitiesToDestroy;

    public void Execute(ref Particles particles, Entity entity)
    {
        if (particles.finishParticles <= elapsedTime)
        {
            entitiesToDestroy.Add(entity);
        }
    }
}