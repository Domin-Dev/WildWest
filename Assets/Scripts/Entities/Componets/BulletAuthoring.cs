using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

public class BulletAuthoring : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float range;
    [SerializeField] private int damage;
    public class Baker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Bullet() { 
                speed = authoring.speed, 
                damage = authoring.damage,
                range = authoring.range 
            });
            AddComponent(entity, new NewBullet());
            AddComponent(entity, new DestroyOnTimer() { value = authoring.range / authoring.speed });
       }
    }
}


[GhostComponent(SendTypeOptimization = GhostSendType.OnlyPredictedClients)]
public struct Bullet : IComponentData
{
    [GhostField] public uint bulletID;
    public float speed;
    public float range;
    public int damage;
}

public struct NewBullet : IComponentData, IEnableableComponent
{
}
