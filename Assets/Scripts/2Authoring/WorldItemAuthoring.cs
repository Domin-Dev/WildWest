using Unity.Entities;
using UnityEngine;

public class WorldItemAuthoring : MonoBehaviour
{
    public class Baker : Baker<WorldItemAuthoring>
    {
        public override void Bake(WorldItemAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<WorldItem>(entity);
            SetComponentEnabled<WorldItem>(entity,false);
        }
    }
}
