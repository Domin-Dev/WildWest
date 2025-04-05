
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct CharacterAnimationSystem : ISystem
{
    private const float maxDistance = 0.005f;
    public const float headOffsetY = 0.13f;

    private const float speedRotationBody = 8f;
    private const float speedRotationHead = 4f;
    private const float speedHead = 2f;


    public void OnUpdate(ref SystemState state)
    {
        float time = (float)SystemAPI.Time.ElapsedTime;

        foreach (RefRO<Character> character in SystemAPI.Query<RefRO<Character>>())
        {
            if (!character.ValueRO.isMove)
            {
                continue;
            }

            LocalTransform bodyTransform = state.EntityManager.GetComponentData<LocalTransform>(character.ValueRO.body);
            LocalTransform headTransform = state.EntityManager.GetComponentData<LocalTransform>(character.ValueRO.headParent);
            
            float localTime = time - character.ValueRO.startAnim;

            float angle = math.sin(localTime * speedRotationBody) * math.radians(8);
            bodyTransform.Rotation = quaternion.RotateZ(angle);

            float posY = math.sin(localTime * speedHead) * maxDistance;
            headTransform.Position = new float3(0, headOffsetY + posY, 0);

            float angleHead = math.sin(localTime * speedRotationHead) * math.radians(-4f);
            headTransform.Rotation = quaternion.RotateZ(angleHead);

            state.EntityManager.SetComponentData(character.ValueRO.body,bodyTransform);
            state.EntityManager.SetComponentData(character.ValueRO.headParent,headTransform);
        }
    }

}



