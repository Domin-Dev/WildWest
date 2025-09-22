using Unity.Entities;
using UnityEngine;



public class EquipmentContainerAuthoring : MonoBehaviour
{
    public class Baker : Baker<EquipmentContainerAuthoring>
    {
        public override void Bake(EquipmentContainerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ContainerComponent());
            AddBuffer<InventorySlot>(entity);
        }
    }
}

