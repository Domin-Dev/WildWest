using Unity.NetCode;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted,OwnerSendType = SendToOwnerType.SendToNonOwner)]
public struct DamageThisTick : ICommandData
{
    public NetworkTick Tick { get; set; }
    public int value;
}