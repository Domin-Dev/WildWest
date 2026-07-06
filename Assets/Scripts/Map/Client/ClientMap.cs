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
        public int widthInChunks = 100;
        public float2 mapOffset = float2.zero;

        public Dictionary<int2, Entity> chunks { private set; get; }
        public Dictionary<int, Transform> renderedChunks { private set; get; }
        public List<Entity> chunksToRender;
      
        private EntityManager entityManager;


        public ClientMap() 
        { 
            chunks = new Dictionary<int2, Entity>();
            chunksToRender = new List<Entity>();
            renderedChunks = new Dictionary<int,Transform>();
            entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
        }
        public void AddChunk(int chunkIndex,Entity chunk)
        {
            int2 key = ChunkIndexToChunkCoordinates(chunkIndex);

            if (!chunks.ContainsKey(key))
            {
                chunks.Add(key, chunk);
                chunksToRender.Add(chunk);
            }
        }
        public void RemoveChunk(int chunkIndex)
        {
            //Debug.Log("usun!!!");
            int2 coords = ChunkIndexToChunkCoordinates(chunkIndex);
            if (chunks.ContainsKey(coords))       
                chunks.Remove(coords);

            if (renderedChunks.ContainsKey(chunkIndex))
            {
                MapVisualization.instance.RemoveMesh(renderedChunks[chunkIndex]);
                renderedChunks.Remove(chunkIndex);
            }
        }

        public bool k = false;
        public ChunkTiles? this[int x,int y]
        { 
            get
            {
                if(x < 0 || y < 0) return null;

                int2 coordinates = MapPosToChunkCoordinates(x, y);
                if (chunks.TryGetValue(coordinates,out Entity clientChunk) && entityManager.Exists(clientChunk))
                {
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
                 int2 pos = ChunkIndexToMapPosition(chunkComponent.chunkIndex);
                 return this[pos.x + x, pos.y + y];  
            }
        }



        public float2 EnginePositionToTileEnginePosition(float2 position)
        {
            return MapPosToEnginePosition(EnginePositionToMapPos(position));
        }
        public int2 EnginePositionToMapPos(float2 position)
        {
            return new int2((int)((position.x - mapOffset.x) / cellSize), (int)((position.y - mapOffset.y) / cellSize));
        }
        public static int2 MapPosToChunkCoordinates(int x,int y)
        {
            return new int2((x / chunkSize), (y / chunkSize));
        }
        public static int2 MapPosToChunkCoordinates(int2 pos)
        {
            return MapPosToChunkCoordinates(pos.x, pos.y);  
        }
        public static int2 MapPosToLocalChunkPos(int x, int y)
        {
            return new int2(x % chunkSize, y % chunkSize);
        }
        public float2 MapPosToEnginePosition(int2 mapPos)
        {
            return new float2((mapPos.x + 0.5f) * cellSize  + mapOffset.x, (mapPos.y + 0.5f) * cellSize + mapOffset.y);
        }



        public static int LocalTilePosToTileIndex(int2 pos)
        {
            return pos.x + pos.y * chunkSize;
        }
        public static int LocalTilePosToTileIndex(int x,int y)
        {
            return LocalTilePosToTileIndex(new int2(x,y));
        }
        public int ChunkCoordiantesToChunkIndex(int2 coords)
        {
            return coords.x + coords.y * widthInChunks;
        }
        public int2 LocalChunkPosToMapPos(Entity chunk, int x, int y)
        {
            var chunkComponent = entityManager.GetComponentData<ChunkComponent>(chunk);
            int2 pos = ChunkIndexToMapPosition(chunkComponent.chunkIndex);
            return new int2(x + pos.x, y + pos.y);
        }
        public int2 ChunkIndexToChunkCoordinates(int chunkIndex)
        {
            return new int2(chunkIndex % widthInChunks, chunkIndex / widthInChunks);
        }
        public int2 ChunkIndexToMapPosition(int chunkIndex)
        {
            return ChunkIndexToChunkCoordinates(chunkIndex) * chunkSize;
        }
        public bool ChunkWasLoaded(int2 chunkCoordinates)
        {
           return renderedChunks.ContainsKey(ChunkCoordiantesToChunkIndex(chunkCoordinates));
        }
        public bool ChunkWasLoaded(int2 chunkCoordinates, out Transform chunk)
        {
            return renderedChunks.TryGetValue(ChunkCoordiantesToChunkIndex(chunkCoordinates),out chunk);
        }
        public bool GetNextChunk(out Entity? clientChunk)
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
        public void AddNewRenderedChunk(Transform transform, int chunkIndex)
        {
            renderedChunks.Add(chunkIndex, transform);
        }
    }
}

