using Unity.Entities;
using Unity.NetCode;




[GhostComponent(SendTypeOptimization = GhostSendType.AllClients, OwnerSendType = SendToOwnerType.SendToOwner)]
public struct PlayerOutfitStats : IComponentData
{
    [GhostField] public OutfitStats stats;
}



[System.Serializable]
public struct OutfitStats
{
    public int armor;
    public int movementSpeed;
    public int insulation;
    public int waterResistance;
    public int aesthetic;
}
