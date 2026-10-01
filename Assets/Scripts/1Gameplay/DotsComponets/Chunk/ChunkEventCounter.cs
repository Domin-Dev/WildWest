

using Unity.NetCode;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct ChunkEventCounter : IInputComponentData
{
    [GhostField(Quantization = 0)] public uint index;
}