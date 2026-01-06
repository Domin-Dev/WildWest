using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;

[System.Serializable]
public class TileData
{
    public int tileID;
    public byte variant;

    public TileData(ChunkTiles chunkTiles)
    {
        this.tileID = chunkTiles.tileID;
        this.variant = chunkTiles.variant;
    }
}
