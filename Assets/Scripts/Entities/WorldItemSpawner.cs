
using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


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
        if (createdCharacters.Length > 0)
        {
            for (int j = createdCharacters.Length - 1; j >= 0; j--)
            {
                Entity entity = createdCharacters[j];   
                var childs = entityManager.GetBuffer<Child>(entity);
                Hands hands = new Hands();
                foreach (var child in childs)
                {
                    if (entityManager.HasComponent<Head>(child.Value))
                    {
                        var spr = entityManager.GetBuffer<Child>(child.Value)[0];
                        var spriteRenderer = entityManager.GetComponentObject<SpriteRenderer>(spr.Value);
                        var mpb = new MaterialPropertyBlock();

                        spriteRenderer.GetPropertyBlock(mpb);
                        mpb.SetInt("_HairIndex", UnityEngine.Random.Range(14, 31));
                        mpb.SetColor("_SkinColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                        mpb.SetColor("_HairColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                       // mpb.SetColor("_SkinColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                        spriteRenderer.SetPropertyBlock(mpb);
                    }
                    else if (entityManager.HasComponent<Body>(child.Value))
                    {
                        var spriteRenderer = entityManager.GetComponentObject<SpriteRenderer>(child.Value);
                        var mpb = new MaterialPropertyBlock();
                        spriteRenderer.GetPropertyBlock(mpb);
                        mpb.SetColor("_SkinColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                        mpb.SetColor("_OuterwearColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                        mpb.SetColor("_UnderwearColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                        mpb.SetColor("_PantsColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                        mpb.SetColor("_ShirtColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                        mpb.SetColor("_BeltColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                        mpb.SetColor("_AccessoryColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                        mpb.SetColor("_BagColor", new Color(UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f), UnityEngine.Random.Range(0.01f, 1f)));
                        mpb.SetInt("_Direction", 0);
                        spriteRenderer.SetPropertyBlock(mpb);
                    }
                    else if (entityManager.HasComponent<MainHand>(child.Value))
                    {
                        hands.main = child.Value;
                        hands.itemInHand = entityManager.GetBuffer<Child>(child.Value)[0].Value;
                    }
                    else if(entityManager.HasComponent<SideHand>(child.Value))
                    {
                        hands.side = child.Value;
                    }
                }
                entityManager.AddComponent<Hands>(entity);
                entityManager.SetComponentData(entity, hands);
                createdCharacters.RemoveAt(j);
            }
        }
   


        if(Input.GetKeyDown(KeyCode.K) && isReady) {

            for (int j = 0; j < 1; j++)
            {
                SpawnCharacter();
                // Spawn(sprite1, new Vector2((i % 40 )* 0.2f, (i / 40) * 1f));
              //  Debug.Log(i);
            }
        }
    }

    private void SpawnCharacter()
    {
        Entity character = entityManager.Instantiate(entitiesReferences.characterEntity);
        entityManager.SetComponentData(character, LocalTransform.FromPosition(new float3((i % 50) * 0.2f, (i / 50) * 0.2f, 0)));
        i++;
        createdCharacters.Add(character);
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