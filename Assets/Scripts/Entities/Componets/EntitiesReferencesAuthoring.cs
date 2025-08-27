using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour
{
    [Header("Game")]
    public GameObject characterPrefab;
    public GameObject chunkPrefab;
    [Header("Prefabs")]
    public GameObject shadowPrefab;
    public GameObject worldItemPrefab;
    public GameObject worldTextPrefab;
    public GameObject buildObjectEntityPrefab;
    public GameObject bulletPrefab;
    [Header("Particle Prefab")]
    public GameObject shotSmoke;
    public GameObject shotFire;
    public GameObject shotLight;
    public GameObject spark;
    public class Baker : Baker<EntitiesReferencesAuthoring>
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                shadowEntity = GetEntity(authoring.shadowPrefab, TransformUsageFlags.Dynamic),
                worldItemEntity = GetEntity(authoring.worldItemPrefab, TransformUsageFlags.Dynamic),
                characterEntity = GetEntity(authoring.characterPrefab, TransformUsageFlags.Dynamic),
                chunkEntity = GetEntity(authoring.chunkPrefab, TransformUsageFlags.Dynamic),
                
                buildObjectEntity = GetEntity(authoring.buildObjectEntityPrefab, TransformUsageFlags.Dynamic),
                worldTextEntity = GetEntity(authoring.worldTextPrefab, TransformUsageFlags.Dynamic),

                shotSmoke = GetEntity(authoring.shotSmoke, TransformUsageFlags.Dynamic),
                bulletEntity = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic),
                shotFire = GetEntity(authoring.shotFire, TransformUsageFlags.Dynamic),
                shotLight = GetEntity(authoring.shotLight, TransformUsageFlags.Dynamic),
                spark = GetEntity(authoring.spark, TransformUsageFlags.Dynamic)
            });
        }
    }
}
public struct EntitiesReferences : IComponentData
{
    public Entity characterEntity;
    public Entity chunkEntity;
    [Space]

    public Entity shadowEntity;
    public Entity worldItemEntity;
    public Entity worldTextEntity;
    [Space]
    public Entity buildObjectEntity;
    public Entity bulletEntity;
    [Space]
    public Entity shotSmoke;
    public Entity shotFire;
    public Entity shotLight;
    [Space]
    public Entity spark;

}
