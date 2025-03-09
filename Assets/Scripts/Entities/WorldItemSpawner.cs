using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;

public class WorldItemSpawner : MonoBehaviour
{
    [SerializeField] Sprite sprite1;

    private EntitiesReferences entitiesReferences;
    private EntityManager entityManager;
    [SerializeField] private SubScene subScenes;
    int i = 0;




    private bool isReady = false;

    void Start()
    {
        InvokeRepeating("WaitForEntity", 0.1f, 0.1f);
    }

    private void WaitForEntity()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(EntitiesReferences));
        if (query.IsEmpty)
            ChatManager.instance.Print("Czekam na załadowanie komponentu EntitiesReferences...");
        else
        {
            SetUp(query);
            CancelInvoke("WaitForEntity");
        }
    }
    private void SetUp(EntityQuery entityQuery)
    {
        ChatManager.instance.Print("Udalo sie wczytac");
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entitiesReferences = entityManager.GetComponentData<EntitiesReferences>(entityQuery.GetSingletonEntity());
        ChatManager.instance.Print("wszystko gotowe");
        isReady = true;
    }
    public void Spawn(Sprite sprite,Vector2 position)
    {
        Entity shadow = entityManager.Instantiate(entitiesReferences.shadowEntity);
        Entity worldItem = entityManager.Instantiate(entitiesReferences.worldItemEntity);

        entityManager.GetComponentObject<SpriteRenderer>(worldItem).sprite = sprite;
        entityManager.AddComponentData(worldItem, new Parent { Value = shadow });
        entityManager.SetComponentData(worldItem, LocalTransform.FromPosition(new float3(0,WorldItemAnimJob.basePos, 0)));
        entityManager.SetComponentData(shadow, LocalTransform.FromPosition(new float3(position.x, position.y, 0)));
        i++;
    }
}