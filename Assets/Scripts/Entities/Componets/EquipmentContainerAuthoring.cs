using Unity.Entities;
using Unity.NetCode;
using UnityEngine;



public class EquipmentContainerAuthoring : MonoBehaviour
{
    public class Baker : Baker<EquipmentContainerAuthoring>
    {

        public override void Bake(EquipmentContainerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new ContainerComponent());
            AddComponent(entity, new EquipmentEventCounter() { index = uint.MaxValue });



            AddBuffer<InventorySlot>(entity);
            AddBuffer<EquipmentEventBuffer>(entity);
        }
    }
}

