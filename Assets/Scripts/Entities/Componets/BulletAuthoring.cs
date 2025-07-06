using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class BulletAuthoring : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private int damage;
    [SerializeField] private float destroyAfterTime;
    public class Baker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Bullet() { speed = authoring.speed, damage = authoring.damage });
            AddComponent(entity, new NewBullet());
            AddComponent(entity, new DestroyOnTimer() {value = authoring.destroyAfterTime});
       }
    }
}

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients)]

public struct Bullet : IComponentData
{
    public float speed;
    public int damage;
}

public struct NewBullet : IComponentData
{
    public bool isOnServer;
}
