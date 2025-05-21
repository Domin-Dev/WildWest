
using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;


public class EntitySpawner : MonoBehaviour
{
    [SerializeField] Sprite sprite1;

    private EntitiesReferences entitiesReferences;
    private EntityManager entityManager;


    public Entity player;
    int counter = 0;


    private NativeList<Entity> createdCharacters;



    private const float playerSpeed = 1.0f;


    private static EntitySpawner i;
    public static EntitySpawner instance
    {
        get
        {
            return i;
        }
    }
    private void Awake()
    {
        if (i == null)
        {
            i = this;
        }
        createdCharacters = new NativeList<Entity>(Allocator.Persistent);
    }

    private bool isReady = false;

    void Start()
    {
        InvokeRepeating("WaitForEntity", 0.1f, 0.1f);
    }

  

    private void OnDestroy()
    {
        createdCharacters.Dispose();
    }
    private Entity GetChild(Entity parent,int depth)
    {
        for (int i = 0; i < depth; i++)
        {
            parent = entityManager.GetBuffer<Child>(parent)[0].Value;
        }
        return parent;
    }
    //public void SpawnPlayer(bool tr, float3 pos)
    //{
    //    Debug.Log("Spawn Player");
    //    Entity character = entityManager.Instantiate(entitiesReferences.characterEntity);
    //    entityManager.SetComponentData(character, LocalTransform.FromPosition(pos));
    //    if (tr)
    //    {
    //        entityManager.AddComponentData(character, new Player() { speed = playerSpeed });
    //        var physics = entityManager.GetComponentData<Physics2D>(character);
    //        entityManager.AddComponentData(character, physics);
    //        player = character;
    //        this.AddComponent<CharacterManager>().SetUp(player);
    //    }
    //   // createdCharacters.Add(character);
    //    counter++;
    //}
    //private void SpawnBuildObject(float3 position,int objectID,int variantIndex)
    //{
    //    VariantItem buildingItem = ItemsAsset.instance.GetItem<VariantItem>(objectID);
    //    Variant variant = buildingItem.objectVariants[variantIndex].variants[0];
        

    //    position.z = position.y;
    //    Entity character = entityManager.Instantiate(entitiesReferences.buildObjectEntity);
    //    entityManager.SetComponentData(character, LocalTransform.FromPosition(position));
    //}
    //private void SpawnCharacter()
    //{
    //    Entity character = entityManager.Instantiate(entitiesReferences.characterEntity);
    //    entityManager.SetComponentData(character, LocalTransform.FromPosition(new float3((counter % 50) * 0.2f, (counter / 50) * 0.2f, 0)));
    //    counter++;
    //    createdCharacters.Add(character);
    //}
    private void WaitForEntity()
    {
        EntityManager entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
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
        Debug.Log("Spraw");
   //     SpawnPlayer(true,float3.zero);
    }
    private void SetUp(EntityQuery entityQuery)
    {
        ChatManager.instance.Print("Udalo sie wczytac");
        entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
        entitiesReferences = entityManager.GetComponentData<EntitiesReferences>(entityQuery.GetSingletonEntity());
        ChatManager.instance.Print("wszystko gotowe");
        isReady = true;
    }
    public void SpawnParticle(int indexParticle, float3 position, quaternion quaternion)
    {
        Entity prefab = GetParticleIndex(indexParticle);
        Entity entity = entityManager.Instantiate(prefab);
        position.z = position.y;
        LocalTransform localTransform = LocalTransform.FromPosition(position);
        entityManager.SetComponentData(entity, localTransform.Rotate(quaternion));
        Particles particles = entityManager.GetComponentData<Particles>(entity);
        particles.finishParticles += Time.time;
        entityManager.SetComponentData(entity, particles);
    }
    private Entity GetParticleIndex(int index)
    {
        switch (index)
        { 
            case 0: return entitiesReferences.shotSmoke;
            case 1: return entitiesReferences.shotFire;
        }
        return Entity.Null;
    }
}