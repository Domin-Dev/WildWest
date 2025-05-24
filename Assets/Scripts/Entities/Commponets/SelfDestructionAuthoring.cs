using Unity.Entities;
using UnityEngine;



public class SelfDestructionAuthoring : MonoBehaviour
{
    [SerializeField] private double timeToSelfDestruction;
    public class Baker : Baker<SelfDestructionAuthoring>
    {
        public override void Bake(SelfDestructionAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SelfDestruction()
            {
                finishParticles = authoring.timeToSelfDestruction
            });
        }
    }
}

public struct SelfDestruction : IComponentData
{
    public double finishParticles;
}
