using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Client.Map
{
    public class ClientMap
    {
        public const int chunkSize = 10;
        public const float cellSize = 0.25f;


        public Dictionary<int2,ClientChunk> chunks { private set; get; }
        public Dictionary<int2, Transform> renderedChunks { private set; get; }
        public List<ClientChunk> chunksToRender;
     


        public ClientMap() 
        { 
            chunks = new Dictionary<int2,ClientChunk>();
            chunksToRender = new List<ClientChunk>();
            renderedChunks = new Dictionary<int2,Transform>();
        }
        public void AddChunk(in FixedChunk chunk)
        {
            if (!chunks.ContainsKey(chunk.chunkCoordinates))
            {
                ClientChunk clientChunk = new ClientChunk(in chunk);
                chunks.Add(chunk.chunkCoordinates, clientChunk);
                chunksToRender.Add(clientChunk);
            }
        }

        public bool k = false;

        public ClientTile this[int x,int y]
        { 
            get
            {
                if(x < 0 || y < 0) return null;

                int2 coordinates = MapPosToChunkCoordinates(x, y);
                if (chunks.ContainsKey(coordinates))
                {
                    ClientChunk clientChunk = chunks[coordinates];
                    int2 localChunkPos = MapPosToLocalChunkPos(x, y);
                    return clientChunk[localChunkPos];
                }
                return null;
            }
        }
        public ClientTile this[ClientChunk chunk,int x, int y]
        {
            get
            {
                  return this[chunk.chunkCoordinates.x + x, chunk.chunkCoordinates.y + y];  
            }
        }
        public static int2 MapPosToChunkCoordinates(int x,int y)
        {
            return new int2((x / chunkSize) * chunkSize, (y / chunkSize) * chunkSize);
        }
        public static int2 MapPosToLocalChunkPos(int x, int y)
        {
            return new int2(x % chunkSize, y % chunkSize);
        }
        public int2 LocalChunkPosToMapPos(ClientChunk chunk,int x,int y)
        {
            return new int2(x + chunk.chunkCoordinates.x, y + chunk.chunkCoordinates.y);
        }

        public bool GetNextChunk(out ClientChunk clientChunk)
        {
            if (chunksToRender.Count > 0)
            {
                clientChunk = chunksToRender[0];
                chunksToRender.RemoveAt(0);
                return true;
            }
            else
            {
                clientChunk = null;
                return false;
            }
        }
        public void AddNewRenderedChunk(Transform transform, int2 chunkCoordinates)
        {
            renderedChunks.Add(chunkCoordinates, transform);
        }
    }
}

