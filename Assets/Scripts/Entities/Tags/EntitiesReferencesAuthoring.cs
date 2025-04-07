using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour
{
    public GameObject shadowPrefab;
    public GameObject worldItemPrefab;
    public GameObject characterPrefab;
    public GameObject buildObjectEntityPrefab;
    public class Baker : Baker<EntitiesReferencesAuthoring>
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences{
                shadowEntity = GetEntity(authoring.shadowPrefab, TransformUsageFlags.Dynamic),
                worldItemEntity = GetEntity(authoring.worldItemPrefab, TransformUsageFlags.Dynamic),
                characterEntity = GetEntity(authoring.characterPrefab, TransformUsageFlags.Dynamic),
                buildObjectEntity = GetEntity(authoring.buildObjectEntityPrefab, TransformUsageFlags.Dynamic),
            });
        }
    }
}
public struct EntitiesReferences : IComponentData
{
    public Entity shadowEntity;
    public Entity worldItemEntity;
    public Entity characterEntity;
    public Entity buildObjectEntity;
}
