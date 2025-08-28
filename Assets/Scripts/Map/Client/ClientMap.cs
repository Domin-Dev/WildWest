using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

namespace Game.Client.Map
{
    public class ClientMap
    {
        public const int chunkSize = 10;
        public const float cellSize = 0.25f;

        public int widthInChunks = 10;

        public Dictionary<int2, Entity> chunks { private set; get; }
        public Dictionary<int2, Transform> renderedChunks { private set; get; }
        public List<Entity> chunksToRender;
      
        private EntityManager entityManager;


        public ClientMap() 
        { 
            chunks = new Dictionary<int2, Entity>();
            chunksToRender = new List<Entity>();
            renderedChunks = new Dictionary<int2,Transform>();
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }
        public void AddChunk(int chunkIndex,Entity chunk)
        {

            int2 key = ChunkIndexToChunkCoordinates(chunkIndex);

            Debug.Log(chunkIndex + "  " + chunk);
            if (!chunks.ContainsKey(key))
            {
                chunks.Add(key, chunk);
                chunksToRender.Add(chunk);
            }
        }

        public bool k = false;

        public ChunkTiles? this[int x,int y]
        { 
            get
            {
                if(x < 0 || y < 0) return null;

                int2 coordinates = MapPosToChunkCoordinates(x, y);
                if (chunks.ContainsKey(coordinates))
                {
                    Entity clientChunk = chunks[coordinates];
                    int index  = LocalTilePosToTileIndex(MapPosToLocalChunkPos(x, y));
                    ChunkTiles tile = entityManager.GetBuffer<ChunkTiles>(clientChunk)[index];
                    return tile;
                }
                return null;
            }
        }
        public ChunkTiles? this[Entity chunk,int x, int y]
        {
            get
            {
                 var chunkComponent = entityManager.GetComponentData<ChunkComponent>(chunk);
                 int2 pos = ChunkIndexToMapPosition(chunkComponent.index);
                 return this[pos.x + x, pos.y + y];  
            }
        }
        public static int2 MapPosToChunkCoordinates(int x,int y)
        {
            return new int2((x / chunkSize), (y / chunkSize));
        }
        public static int2 MapPosToLocalChunkPos(int x, int y)
        {
            return new int2(x % chunkSize, y % chunkSize);
        }
        public static int LocalTilePosToTileIndex(int2 pos)
        {
            return pos.x + pos.y * chunkSize;
        }
        public static int LocalTilePosToTileIndex(int x,int y)
        {
            return LocalTilePosToTileIndex(new int2(x,y));
        }

        public int2 LocalChunkPosToMapPos(ClientChunk chunk,int x,int y)
        {
            return new int2(x + chunk.chunkCoordinates.x, y + chunk.chunkCoordinates.y);
        }
        public int2 ChunkIndexToChunkCoordinates(int chunkIndex)
        {
            return new int2(chunkIndex % widthInChunks, chunkIndex / widthInChunks);
        }
        public int2 ChunkIndexToMapPosition(int chunkIndex)
        {
            return ChunkIndexToChunkCoordinates(chunkIndex) * chunkSize;
        }


        public bool GetNextChunk(out Entity? clientChunk)
        {
            foreach (var item in chunksToRender)
            {
                Debug.Log(item);
            }

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

