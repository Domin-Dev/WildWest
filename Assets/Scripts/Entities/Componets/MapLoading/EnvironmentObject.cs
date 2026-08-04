

using Unity.Entities;

public struct EnvironmentObject : IComponentData
{
    
}
public struct ClientGridObject : IComponentData
{
    public Entity sprite;
    public Entity shadow;
}