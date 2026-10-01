using Unity.Entities;
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