using Unity.Mathematics;
using Unity.NetCode;

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct PlayerInput : IInputComponentData
{
    [GhostField(Quantization = 0)] public float2 movementDirection;
    [GhostField(Quantization = 0)] public float2 sightDirection;
    [GhostField(Quantization = 0)] public InputEvent rightButton;
    [GhostField(Quantization = 0)] public InputEvent leftButton;
    [GhostField(Quantization = 0)] public InputEvent reloadButton;
    [GhostField(Quantization = 0)] public InputEvent unloadButton;


    [GhostField(Quantization = 0)] public int slotInHand;
    [GhostField(Quantization = 0)] public int ammoSelectedIndex;

    [GhostField(Quantization = 0)] public NetworkTick dataTick;
    
    public bool SightDirectionIsEmpty()
    {
        return sightDirection.x == float.MinValue && sightDirection.y == float.MinValue;
    }
}