using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
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
            AddBuffer<ChunkTiles>(entity);
            AddBuffer<BuildingObjects>(entity);
            AddBuffer<LocalBuildingObjects>(entity);
            AddBuffer<GhostChildren>(entity);
            AddBuffer<EntityContainers>(entity);
            AddComponent<NextTempIndex>(entity, new NextTempIndex() {
                    nextTempIndex = EquipmentConfig.start_ContainerIndex
            });
            AddBuffer<WorldItemPosition>(entity);
        }
    }
}


[GhostComponent(OwnerSendType = SendToOwnerType.All)]
public struct WorldItemPosition : IBufferElementData,IGetSlot
{
    [GhostField] public float2 worldItemPos;
    [GhostField] public int slot;

    public void SetSlot(int slot)
    {
        this.slot = slot;
    }
    public int GetSlot() { return slot; }

}