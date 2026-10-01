
using Unity.NetCode;

[GhostComponent(PrefabType = GhostPrefabType.All)]
public struct EquipmentEventCounter : IInputComponentData
{
    [GhostField(Quantization = 0)] public uint index;

}