using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;





public struct HitScaleAnimation : IComponentData
{
    public float Timer;
    public float Duration;
    public float MaxScale;
    public float OriginalScale;
}

public struct HitRotationAnimation : IComponentData
{
    public float Timer;
    public float Duration;
    public float RotationAngle;
    public float OriginalRotation;
}

public struct HitAnimationEvent : IComponentData
{
    public Entity entity;
    public HitScaleAnimation scaleAnim;
    public HitRotationAnimation rotationAnim;
}


[BurstCompile]
[UpdateInGroup(typeof(PresentationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct HitAnimationSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float dt = SystemAPI.Time.DeltaTime;
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (transform, anim, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<HitScaleAnimation>>().WithEntityAccess())
        {
            anim.ValueRW.Timer += dt;
            float t = math.saturate(anim.ValueRW.Timer / anim.ValueRW.Duration);
            float pulse = math.sin(t * math.PI);
            transform.ValueRW.Scale = math.lerp(anim.ValueRW.OriginalScale, anim.ValueRW.MaxScale, pulse);

            if (t >= 1f)
            {
                transform.ValueRW.Scale = anim.ValueRO.OriginalScale;
                ecb.RemoveComponent<HitScaleAnimation>(entity);
            }
        }

        foreach (var (transform, anim, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<HitRotationAnimation>>().WithEntityAccess())
        {
            anim.ValueRW.Timer += dt;
            float t = math.saturate(anim.ValueRW.Timer / anim.ValueRW.Duration);
            float pulse = math.sin(t * math.PI);

            float rotationAmount = math.radians(anim.ValueRW.RotationAngle);
            float rotation = pulse * rotationAmount;
            transform.ValueRW.Rotation = quaternion.EulerXYZ(0f, 0f,rotation);

            if (t >= 1f)
            {
                transform.ValueRW.Rotation =  quaternion.EulerXYZ(0f, 0f, anim.ValueRO.OriginalRotation);
                ecb.RemoveComponent<HitRotationAnimation>(entity);
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}