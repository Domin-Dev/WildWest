using Unity.Entities;

public struct CurrentPlayerState : IComponentData
{
    public PlayerState state;
}