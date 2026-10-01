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
            if(!authoring.main.loop)
            {
                AddComponent(entity, new DestroyOnTimer()
                {
                    value = authoring.main.duration,
                });
            }
            AddComponent(entity, new NewParticles());
        }
    }
}