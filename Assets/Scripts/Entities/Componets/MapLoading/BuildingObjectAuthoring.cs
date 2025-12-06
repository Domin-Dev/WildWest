using Unity.Entities;
using UnityEngine;
using UnityEngine.TextCore.Text;



public class BuildingObjectAuthoring : MonoBehaviour
{
    public class Baker : Baker<BuildingObjectAuthoring>
    {
        public override void Bake(BuildingObjectAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<NewBuildingObject>(entity);
        }
    }
}

