using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TileData
{
    public int tileID;
    public byte variant;

    public TileData(ChunkTiles tiles)
    {
        this.tileID = tiles.tileID;
        this.variant = tiles.variant;
    }
}