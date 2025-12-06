using Unity.Entities;
using UnityEngine;
using UnityEngine.TextCore.Text;



public class ChunkManagerAuthoring : MonoBehaviour
{
    public class Baker : Baker<ChunkManagerAuthoring>
    {
        public override void Bake(ChunkManagerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddBuffer<ChunkEvents>(entity);
            AddComponent(entity, new ChunkEventCounter() { index = uint.MaxValue });
        }
    }
}

