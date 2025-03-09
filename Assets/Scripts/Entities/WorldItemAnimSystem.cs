using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial struct WorldItemAnimSystem : ISystem
{

    private float time;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        time += SystemAPI.Time.DeltaTime;
        WorldItemAnimJob job = new WorldItemAnimJob()
        {
            elapsedTime = time
        };
        job.ScheduleParallel();

        //foreach (RefRW<LocalTransform> localTransform in SystemAPI.Query<RefRW<LocalTransform>>().WithPresent<WorldItemAnim>())
        //{
        //    float pingPongValue = math.sin(elapsedTime / cycleTime * math.PI); 
        //    float targetY = pingPongValue * maxDistance;
        //    localTransform.ValueRW.Position = new float3(localTransform.ValueRW.Position.x, basePos + targetY, localTransform.ValueRW.Position.y); 
        //}
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
  }


}


[BurstCompile]
public partial struct WorldItemAnimJob : IJobEntity
{
    private const float cycleTime = 0.5f;
    private const float maxDistance = 0.007f;
    public const float basePos = -0.01f;

    public float elapsedTime;
    public void Execute(ref LocalTransform localTransform, WorldItemAnim worldItemAnim)
    {
        float pingPongValue = math.sin(elapsedTime / cycleTime * math.PI);
        float targetY = pingPongValue * maxDistance;
        localTransform.Position = new float3(localTransform.Position.x, basePos + targetY, localTransform.Position.y);
    }
}
