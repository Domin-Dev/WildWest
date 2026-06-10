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
            AddComponent(entity, new NewChunk());
            AddComponent(entity, new ChunkTag());
            AddBuffer<ChunkTiles>(entity);
            AddBuffer<BuildingObjects>(entity);
            AddBuffer<GhostChildren>(entity);
            AddBuffer<EntityContainers>(entity);   
        }
    }
}

