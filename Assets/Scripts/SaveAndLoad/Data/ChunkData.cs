using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;

[System.Serializable]
public class ChunkData
{
    public int chunkIndex;
    public TileData[] tiles;
}
