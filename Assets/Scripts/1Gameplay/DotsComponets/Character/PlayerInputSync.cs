using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[GhostComponent(SendTypeOptimization = GhostSendType.AllClients,OwnerSendType = SendToOwnerType.SendToNonOwner)]
public struct PlayerInputSync : IComponentData
{
    [GhostField] public float2 movementDir;
    [GhostField] public float2 sightPosition;
    
    [GhostField] public InputEvent rightButton;
    [GhostField] public InputEvent leftButton;



    public int slotInHand; 
    public int ammoSelectedIndex;
    public int ammoSelectedItemID;
    public int ammoSelectedTagID;
}