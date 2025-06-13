using Unity.Entities;
using UnityEngine;



public class SelfDestructionAuthoring : MonoBehaviour
{
    [SerializeField] private float timeToSelfDestruction;
    public class Baker : Baker<SelfDestructionAuthoring>
    {
        public override void Bake(SelfDestructionAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DestroyOnTimer()
            {
                value = authoring.timeToSelfDestruction
            });
        }
    }
}

