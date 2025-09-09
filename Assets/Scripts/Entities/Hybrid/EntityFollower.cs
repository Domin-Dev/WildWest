using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

public class EntityFollower : MonoBehaviour
{
    public Entity entity;
    private EntityManager entityManager;
    private bool followZ;

    void Start()
    { 
        entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
    }

    public void SetEntity(Entity entity, bool followZ)
    {
        this.entity = entity;
        this.followZ = followZ;
    }

    void Update()
    {
        if (entityManager.Exists(entity) && entityManager.HasComponent<LocalTransform>(entity))
        {
            var position = entityManager.GetComponentData<LocalTransform>(entity).Position;
            if(!followZ) position.z = transform.position.z;
            transform.position = position;
        }
        else
        {
            HybridManager.instance.EntityDeleted(entity);
        }
    }
}
