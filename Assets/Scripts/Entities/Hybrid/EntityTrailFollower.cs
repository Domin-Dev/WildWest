using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(CharacterAimSystem))]
public class EntityTrailFollower : MonoBehaviour
{
    public Entity entity;
    private EntityManager entityManager;

    [SerializeField] private int en;
    [SerializeField] private int ver;
    void Start()
    {
        entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
    }

    public void SetEntity(Entity entity)
    {
        en = entity.Index;
        ver = entity.Version;
        this.entity = entity;
    }

    void Update()
    {
        if (entityManager.Exists(entity))
        {
            var position = entityManager.GetComponentData<LocalTransform>(entity).Position;
            transform.position = position;
        }
        else
        {
       //     Destroy(gameObject);
        }
    }
}
