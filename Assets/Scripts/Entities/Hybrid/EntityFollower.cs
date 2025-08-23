using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

public class EntityFollower : MonoBehaviour
{
    public Entity entity;
    private EntityManager entityManager;

    void Start()
    { 
        entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
    }

    public void SetEntity(Entity entity)
    {
        this.entity = entity;
    }

    void Update()
    {
        if (entityManager.Exists(entity) && entityManager.HasComponent<LocalTransform>(entity))
        {
            var position = entityManager.GetComponentData<LocalTransform>(entity).Position;
            transform.position = position;
        }
        else
        {
            HybridManager.instance.EntityDeleted(entity);
        }
    }
}
