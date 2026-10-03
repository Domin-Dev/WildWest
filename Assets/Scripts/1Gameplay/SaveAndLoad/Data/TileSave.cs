using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TileSave
{
    public int tileID;
    public byte variant;

    public TileSave(ChunkTiles tiles)
    {
        this.tileID = tiles.tileID;
        this.variant = tiles.variant;
    }
}