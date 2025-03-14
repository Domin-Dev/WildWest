using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldItemSpawner : MonoBehaviour
{
    [SerializeField] Sprite sprite1;

    private EntitiesReferences entitiesReferences;
    private EntityManager entityManager;
    int i = 0;


    private NativeList<Entity> createdCharacters;

    
   


    private void Awake()
    {
        createdCharacters = new NativeList<Entity>(Allocator.Persistent);
    }

    private bool isReady = false;

    void Start()
    {
        InvokeRepeating("WaitForEntity", 0.1f, 0.1f);
    }

    private void Update()
    {

        if(Input.GetKeyDown(KeyCode.K) && isReady) {

            for (int j = 0; j < 100; j++)
            {
                SpawnCharacter();
                // Spawn(sprite1, new Vector2((i % 40 )* 0.2f, (i / 40) * 1f));
                Debug.Log(i);
            }
        }
    }

    private void SpawnCharacter()
    {
        Entity character = entityManager.Instantiate(entitiesReferences.characterEntity);
        entityManager.SetComponentData(character, LocalTransform.FromPosition(new float3((i % 50) * 0.2f, (i / 50) * 0.2f, 0)));
        i++;
       // Debug.Log(entityManager.GetBuffer<Child>(character));


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
        Entity character = entityManager.Instantiate(entitiesReferences.characterEntity);

        entityManager.GetComponentObject<SpriteRenderer>(worldItem).sprite = sprite;
        entityManager.AddComponentData(worldItem, new Parent { Value = shadow });
        entityManager.SetComponentData(worldItem, LocalTransform.FromPosition(new float3(0,WorldItemAnimJob.basePos, 0)));
        entityManager.SetComponentData(shadow, LocalTransform.FromPosition(new float3(position.x, position.y, 0)));

        entityManager.SetComponentData(character, LocalTransform.FromPosition(new float3(position.x, position.y + 0.5f, 0)));
        i++;
    }
}