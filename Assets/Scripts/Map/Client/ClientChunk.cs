using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;

namespace Game.Client.Map
{
    public class ClientChunk
    {
        public ClientTile[,] tiles { private set; get; }
        public int2 chunkCoordinates { private set; get; }
        public float2 worldPosition { private set; get; }
        public ClientChunk(in FixedChunk fixedChunk)
        {
            chunkCoordinates = fixedChunk.chunkCoordinates;
            worldPosition = fixedChunk.worldPosition;
            tiles = new ClientTile[ClientMap.chunkSize, ClientMap.chunkSize];

            for (int i = 0; i < ClientMap.chunkSize; i++)
            {
                for (int j = 0; j < ClientMap.chunkSize; j++)
                {
                    tiles[j, i] =  new ClientTile(fixedChunk[j, i]);
                }
            }
        }
        public ClientTile this[int x, int y]
        {
            get
            {
                if(x >= 0 && y >= 0 && x < ClientMap.chunkSize && y < ClientMap.chunkSize)
                    return tiles[x, y];
                else 
                    return null;
            }
        }
        public ClientTile this[int2 localPos]
        {
            get
            {
                return this[localPos.x, localPos.y];
            }
        }
    }
}
