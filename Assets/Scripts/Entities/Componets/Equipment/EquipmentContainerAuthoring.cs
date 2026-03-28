using Unity.Entities;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;



public class EquipmentContainerAuthoring : MonoBehaviour
{
    public class Baker : Baker<EquipmentContainerAuthoring>
    {
        public override void Bake(EquipmentContainerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new ContainerComponent());
            AddComponent(entity, new EquipmentEventCounter() { index = uint.MaxValue });
           
            AddComponent(entity, new ToSave());
            SetComponentEnabled<ToSave>(entity,false);
            
            AddBuffer<InventorySlot>(entity);
            AddBuffer<ItemBarData>(entity);
            AddBuffer<EquipmentEventBuffer>(entity);
            AddBuffer<LinkedContainers>(entity);
        }
    }
}
