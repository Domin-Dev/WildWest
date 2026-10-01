using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct WorldItemsConfig: IComponentData
{
    public CollisionFilter FilterToFindSimilarWorldItems;
    public CollisionFilter FilterToFindEnvironment;
    public float2 sizeWorldItemCollider;

    public float dropRangeMin;
    public float dropRangeMax;
}