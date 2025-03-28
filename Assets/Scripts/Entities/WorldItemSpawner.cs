
using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;


public class WorldItemSpawner : MonoBehaviour
{
    [SerializeField] Sprite sprite1;

    private EntitiesReferences entitiesReferences;
    private EntityManager entityManager;
    private BlobAssetStore blobAssetStore;


    public Mesh quadMesh; // ✅ Siatka dla encji (np. kwadrat)
    public UnityEngine.Material entityMaterial; // ✅ Materiał encji


    int i = 0;


    private NativeList<Entity> createdCharacters;



    private const float playerSpeed = 2.0f;

    private void Awake()
    {
        createdCharacters = new NativeList<Entity>(Allocator.Persistent);
        blobAssetStore = new BlobAssetStore();
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
                SpawnPlayer(false, new float3((i % 40) * 0.2f, (i / 40) * 1f,0));
            }
        }
    }
    private void SpawnEntity()
    {
        // 1️⃣ Tworzymy archetyp encji
        EntityArchetype archetype = entityManager.CreateArchetype(
            typeof(LocalTransform),
            typeof(RenderMesh),      // ✅ Dodajemy grafikę (Mesh)
            typeof(RenderBounds),    // ✅ Potrzebne do renderowania
            typeof(LocalToWorld),
            typeof(PhysicsVelocity),
            typeof(PhysicsMass),
            typeof(PhysicsDamping),
            typeof(PhysicsGravityFactor),
            typeof(Simulate),
            typeof(PhysicsCollider)
        );

        // 2️⃣ Tworzymy encję
        Entity character = entityManager.CreateEntity(archetype);
        entityManager.SetComponentData(character, LocalTransform.FromPosition(new float3(0, 2, 0)));

        // 3️⃣ Dodajemy fizykę (Rigidbody 2D)
        entityManager.SetComponentData(character, PhysicsMass.CreateDynamic(new Unity.Physics.MassProperties(), 1f));
        entityManager.SetComponentData(character, new PhysicsDamping { Linear = 0.05f, Angular = 0.05f });
        entityManager.SetComponentData(character, new PhysicsGravityFactor { Value = 1f });

        // 4️⃣ Tworzymy BoxCollider
        BlobAssetReference<Unity.Physics.Collider> collider = Unity.Physics.BoxCollider.Create(new BoxGeometry
        {
            Center = float3.zero,
            Size = new float3(1f, 1f, 0.1f),
            Orientation = quaternion.identity,
            BevelRadius = 0f
        });
        entityManager.SetComponentData(character, new PhysicsCollider { Value = collider });

       
        entityManager.SetSharedComponentManaged(character, new RenderMesh
        {
            mesh = quadMesh,
            material = entityMaterial
        });

        // 6️⃣ Ustawiamy RenderBounds (potrzebne dla renderera)
        entityManager.SetComponentData(character, new RenderBounds
        {
            Value = new AABB { Center = float3.zero, Extents = new float3(0.5f, 0.5f, 0.1f) }
        });

        Debug.Log("Stworzono encję z grafiką i fizyką 2D!");
    }

    private void OnDestroy()
    {
        blobAssetStore.Dispose();
    }

    private void SpawnPlayer(bool tr , float3 pozycja)
    {
        Entity character = entityManager.Instantiate(entitiesReferences.characterEntity);
        entityManager.SetComponentData(character, LocalTransform.FromPosition(pozycja));
        if(tr) entityManager.AddComponentData(character, new Player() { speed = playerSpeed });
        createdCharacters.Add(character);
        i++;
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