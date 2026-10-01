using Unity.Entities;

public struct SynchronizeRelevancyWithParent : IComponentData
{
    public Entity parent;
}
