using Unity.Entities;

public struct ClientGridObject : IComponentData
{
    public Entity sprite;
    public Entity shadow;
}