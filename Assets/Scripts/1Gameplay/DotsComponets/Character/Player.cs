using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients)]
public struct Player : IComponentData
{
    [GhostField] public float speed;
    [GhostField] public FixedString128Bytes playerName;

    [GhostField] public float armor; 
    [GhostField] public float insulation;
    [GhostField] public float waterResistance;
    [GhostField] public float aesthetic;
    [GhostField] public float wetness;
}