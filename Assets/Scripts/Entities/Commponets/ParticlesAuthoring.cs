using Unity.Entities;
using UnityEngine;



public class ParticlesAuthoring : MonoBehaviour
{
    public class Baker : Baker<ParticleSystem>
    {
        public override void Bake(ParticleSystem authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            double finish;
            if(authoring.loop)
            {
                finish = -1;
            }
            else
            {
                finish = authoring.main.duration;
            }
            AddComponent(entity, new SelfDestruction()
            {
                finishParticles = finish,
            });
        }
    }
}

