using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;



public class ParticlesAuthoring : MonoBehaviour
{
    public class Baker : Baker<ParticleSystem>
    {
        public override void Bake(ParticleSystem authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            float finish;
            if(authoring.loop)
            {
                finish = -1;
            }
            else
            {
                finish = authoring.main.duration;
            }
            AddComponent(entity, new DestroyOnTimer()
            {
                value = finish,
            });
            AddComponent(entity, new NewParticles());
        }
    }
}


public struct NewParticles : IComponentData
{
    public Entity target;
    public float3 offset;
}
