
using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;


public class WorldItemSpawner : MonoBehaviour
{
    [SerializeField] Sprite sprite1;

    private EntitiesReferences entitiesReferences;
    private EntityManager entityManager;


    public Entity player;
    int i = 0;


    private NativeList<Entity> createdCharacters;



    private const float playerSpeed = 1.0f;

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
                Character character = new Character();


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

                        character.headParent = child.Value;
                        character.head = spr.Value;
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
                        //character.directionHead
                        spriteRenderer.SetPropertyBlock(mpb);

                        character.body = child.Value;
                    }
                    else if (entityManager.HasComponent<MainHand>(child.Value))
                    {
                        hands.main = child.Value;
                        hands.itemInHand = GetChild(child.Value, 4);
                        hands.mainhand = GetChild(child.Value, 1);
                    }
                    else if (entityManager.HasComponent<SideHand>(child.Value))
                    {
                        hands.side = child.Value;
                        hands.sidehand = GetChild(child.Value, 1);
                    }
                }
                entityManager.SetComponentData(entity, character);
                entityManager.SetComponentData(entity, hands);
                createdCharacters.RemoveAt(j);
            }
        }
   
        if(Input.GetKeyDown(KeyCode.K) && isReady) {

            for (int j = 0; j < 6; j++) 
            {
                SpawnPlayer(false, new float3( i * 0.13f + 0.2f,( i %1)* 0.13f + 0.2f,0));
            }
        }
    }

    private Entity GetChild(Entity parent,int depth)
    {
        for (int i = 0; i < depth; i++)
        {
            parent = entityManager.GetBuffer<Child>(parent)[0].Value;
        }
        return parent;
    }
    private void SpawnPlayer(bool tr, float3 pos)
    {
        Entity character = entityManager.Instantiate(entitiesReferences.characterEntity);
        entityManager.SetComponentData(character, LocalTransform.FromPosition(pos));
        if (tr)
        {
            entityManager.AddComponentData(character, new Player() { speed = playerSpeed });
            var physics = entityManager.GetComponentData<Physics2D>(character);
            entityManager.AddComponentData(character, physics);
            player = character;
            this.AddComponent<CharacterManager>().SetUp(player);
        }
        createdCharacters.Add(character);
        i++;
    }

    private void SpawnBuildObject(float3 position)
    {

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
            StartGame();
        }
    }
    private void StartGame()
    {
        SpawnPlayer(true,float3.zero);
    }
    private void SetUp(EntityQuery entityQuery)
    {
        ChatManager.instance.Print("Udalo sie wczytac");
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entitiesReferences = entityManager.GetComponentData<EntitiesReferences>(entityQuery.GetSingletonEntity());
        ChatManager.instance.Print("wszystko gotowe");
        isReady = true;
    }
}