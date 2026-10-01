using Unity.Entities;
using Unity.NetCode;

public struct EntityContainers : IBufferElementData
{
   [GhostField] public Entity entity;
   [GhostField] public int index;
}
