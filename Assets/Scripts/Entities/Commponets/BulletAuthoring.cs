using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

public class BulletAuthoring : MonoBehaviour
{
    public class Baker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Bullet() { speed = 5f, time = -1});
            AddComponent(entity, new NewBullet());
       }
    }
}

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients)]

public struct Bullet : IComponentData
{
    public float speed;
    [GhostField] public float time;
}

public struct NewBullet : IComponentData
{
}
