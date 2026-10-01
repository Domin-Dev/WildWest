
using Unity.Entities;
using UnityEngine;

public struct EntitiesReferences : IComponentData
{
    public Entity characterEntity;
    public Entity chunkEntity;
    public Entity equipmentContainerEntity;
    [Space]

    public Entity shadowEntity;
    public Entity worldItemEntity;
    public Entity worldTextEntity;
    [Space]
    public Entity buildObjectEntity;
    public Entity bulletEntity;
    [Space]
    public Entity shotSmoke;
    public Entity shotFire;
    public Entity shotLight;
    [Space]
    public Entity spark;
}
