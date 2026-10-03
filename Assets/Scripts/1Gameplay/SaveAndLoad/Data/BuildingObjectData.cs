using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BuildingObjectSave
{
    public int x;
    public int y;
    public int  id;
    public short variantIndex;
    public short stateIndex;

    public float hitPoints;
    public float maxHitPoints;
    public BuildingObjectSave(BuildingObjects objects)
    {
        this.x = objects.globalTilePos.x;
        this.y = objects.globalTilePos.y;

        this.id = objects.id;
        this.variantIndex = objects.variantIndex;
        this.stateIndex = objects.stateIndex;

        this.hitPoints = objects.hitPoints;
        this.maxHitPoints = objects.maxHitPoints;
    }
}