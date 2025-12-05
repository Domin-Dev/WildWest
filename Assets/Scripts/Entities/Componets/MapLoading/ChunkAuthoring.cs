using Unity.Entities;
using UnityEngine;



public class ChunkAuthoring : MonoBehaviour
{
    public class Baker : Baker<ChunkAuthoring>
    {
        public override void Bake(ChunkAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ChunkComponent());
            AddBuffer<ChunkTiles>(entity);
            AddBuffer<BuildingObjects>(entity);
            AddBuffer<ChunkObjects>(entity);
        }
    }
}

