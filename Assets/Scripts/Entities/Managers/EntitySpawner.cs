
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
   //     Debug.Log("Spraw");
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
    }


    public void SpawnParticle(int indexParticle, float3 position, quaternion quaternion,NewParticles target)
    {
        Entity prefab = GetParticleIndex(indexParticle);

        Entity entity = entityManager.Instantiate(prefab);
        position.z = position.y;
        LocalTransform localTransform = LocalTransform.FromPosition(position);
        entityManager.SetComponentData(entity, localTransform.Rotate(quaternion));
        entityManager.SetComponentData(entity, target);
    }


    public void SpawnEntityPrefab(int index,float3 position, quaternion quaternion)
    {
        Entity prefab = GetParticleIndex(index);
        Entity entity = entityManager.Instantiate(prefab);
        position.z = position.y;
        LocalTransform localTransform = LocalTransform.FromPosition(position);
     //   SelfDestruction selfD = entityManager.GetComponentData<SelfDestruction>(entity);
      //  selfD.finishParticles += Time.time;
      //  entityManager.SetComponentData(entity, selfD);
    }


    private Entity GetParticleIndex(int index)
    {
        switch (index)
        { 
            case 0: return entitiesReferences.shotSmoke;
            case 1: return entitiesReferences.shotFire;
            case 2: return entitiesReferences.shotLight;
            case 3: return entitiesReferences.spark;
        }
        return Entity.Null;
    }
}