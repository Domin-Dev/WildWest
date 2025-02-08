

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;


public class PlayerPositionArgs : EventArgs
{
    public Vector2 playerPosition;
    public int chunkIndex;
    public Vector2 chunkCoordinates;
    public PlayerPositionArgs(Vector2 position,int chunkIndex,Vector2 chunkCoordinates)
    {
        this.playerPosition = position;
        this.chunkIndex = chunkIndex;
        this.chunkCoordinates = chunkCoordinates;
    }
}
public class TileUV
{
    public TileUV(Vector2 uv00,int variants = 1, Vector2? uv00Grass = null)
    {
        this.uv00 = uv00;
        this.variants = variants;
        this.uv00Grass = uv00Grass; 
    }
    public override string ToString()
    {
        return $"UV00:{uv00} Variants:{variants} UV00Grass{uv00Grass}";
    }

    public Vector2 uv00 { private set; get; }
    public int variants { private set; get; }
    public Vector2? uv00Grass { private set; get; }
}


[System.Serializable]
public class LoadedChunk
{
    public int loadedTime;
    public Transform transform;

    public LoadedChunk(int loadedTime,Transform transform)
    {
        this.loadedTime = loadedTime;
        this.transform = transform;
    }
}

public class GridVisualization : MonoBehaviour
{
    public bool LoadAllMap = true;

    [SerializeField] public GameObject worldItem;
    public const int renderChunks = 2;
    public const int maxLoadedChunks = 30;

    public Map map;
    public Pathfinding pathfinding;
    public Dictionary<int, LoadedChunk> loadedChunks { private set; get; }

    public int lastPlayerChunk { private set; get; } = -1;
    public Vector2 lastPlayerPosition { private set; get; } = Vector2.zero;

    public event EventHandler<PlayerPositionArgs> onPlayerMove;
    public event EventHandler<PlayerPositionArgs> onChangeChunk;

    public Dictionary<int, TileUV> TilesUV { get; private set; }
    public MapGeneratorSettings id { private set; get; }

    int textureWidth;
    int textureHeight;
    const int sizeTile = 25;//px
    float tileWidth;//    sizeTile /textureWidth;
    float tileHeight;//   sizeTile / textureHeight;

    float linetileWidth;

    Texture2D mapTexture;
    float width1;
    float height1;

    public static GridVisualization instance { private set; get; }

    [SerializeField] private Material mapMaterial;
    [SerializeField] private Texture2D linesTexture;
    private void Awake()
    {
        Application.targetFrameRate = -1;
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        id = Resources.Load<MapGeneratorSettings>("mapGeneratorSettings");
        SetUpMapMaterial();
    }



    private void SetUpMapMaterial(int sizeTile = 25)
    {
        TilesUV = new Dictionary<int, TileUV>();
        Floor[] array = ItemsAsset.instance.GetItemsByType<Floor>();
        Texture2D texture = new Texture2D(46 * sizeTile, sizeTile * (CountTextures(array) + 1));
        texture.filterMode = FilterMode.Point;

        textureWidth = texture.width;
        textureHeight = texture.height;
        tileWidth = (float)sizeTile / textureWidth;
        tileHeight = (float)sizeTile / textureHeight;
        width1 = 0.01f / textureWidth;
        height1 = 0.01f / textureHeight;

        //Set Up Lines
        Vector2 uv00 = new Vector2(0,0);
        int variants = linesTexture.width / sizeTile;
        TilesUV.Add(-1, new TileUV(uv00, variants,null));
        linetileWidth = (float)sizeTile / linesTexture.width;
        //

        int k = 0;
        for (int i = 0; i < array.Length; i++)
        {
            Floor floor = array[i];
            Vector2? grassUV = null;
            if (floor.grassTexture != null)
            {
                grassUV = new Vector2(0, (float)k * sizeTile / textureHeight);
                CopyTexture(ref k, sizeTile, floor.grassTexture, texture);
            }
            uv00 = new Vector2(0, (float)k * sizeTile / textureHeight);
            CopyTexture(ref k, sizeTile, floor.texture, texture);
            variants = floor.texture.width / sizeTile;

            if(floor.ID >= 0) TilesUV.Add(floor.ID, new TileUV(uv00, variants, grassUV));
        }
        texture.Apply(true, true);
        mapTexture = texture;
    }
    private int CountTextures(Floor[] array)
    {
        int counter = 0;
        foreach (Floor floor in array)
        {
            if (floor.texture != null)
            {
                counter++;
                if (floor.grassTexture != null)
                {
                    counter++;
                }
            }
        }
        return counter;
    }
    private void CopyTexture(ref int index, int height, Texture2D from, Texture2D to)
    {
        int width = from.width;
        Color32[] grassColors = from.GetPixels32();
        to.SetPixels32(0, index * height, width, height, grassColors);
        index++;
    }
    public void SetMap(Map map)
    {
        this.map = map;
        loadedChunks = new Dictionary<int, LoadedChunk>();
        if (LoadAllMap)
        {
            foreach (var item in map.chunks)
            {
                CreateMesh(item.Value);
            }
        }
        else
        {
            CheckChunks(Vector2.zero);
        }
        pathfinding = new Pathfinding(this);
    }
    //Check current chunk
    //return coordinates of player chunk
    private Vector2 CheckChunks(Vector2 worldPosition)
    {
        Vector2 positionXY = GetGridPosition(worldPosition);
        if (lastPlayerPosition != positionXY)
        {
            CheckGridTile(positionXY);
            lastPlayerPosition = positionXY;
        }

        int chunkIndex = GetChunkIndexByPositionXY(positionXY);

        if (lastPlayerChunk != chunkIndex && !LoadAllMap)
        {
            lastPlayerChunk = chunkIndex;
            Vector2 posChunk = GetChunkCoordinates(chunkIndex);
            StartCoroutine(LoadChunks(posChunk));

            StartCoroutine(TryUnloadChunks(chunkIndex));
            onChangeChunk?.Invoke(this, new PlayerPositionArgs(positionXY, chunkIndex, GetChunkCoordinates(chunkIndex)));
            return posChunk;
        }
        return GetChunkCoordinates(lastPlayerChunk);
    }

    private void CheckGridTile(Vector2 newPlayerPosition)
    {
        GridTile gridTile = GetGridTileByPositionXY(lastPlayerPosition);
        gridTile?.TurnOnObjectsCovering();
        gridTile = GetGridTileByPositionXY(newPlayerPosition);
        gridTile?.TrunOffObjectsCovering();
    }

    IEnumerator LoadChunks(Vector2 posChunk)
    {  
        List<int> list = new List<int>();
        for (int x = -renderChunks; x <= renderChunks; x++)
        {
            for (int y = -renderChunks; y <= renderChunks; y++)
            {
               int index = GetChunkIndexByCoordinates(posChunk + new Vector2(x, y));     
               if(CheckChunk(index))
               {
                    list.Add(index);
               }
               else
               {
                   if(loadedChunks.ContainsKey(index)) loadedChunks[index].loadedTime = (int)Time.time;
               }
            }
        }

        foreach (int index in list)
        {
            StartCoroutine(LoadChunk(index));
            StartCoroutine(TryUnloadChunks(lastPlayerChunk));

            yield return null;

        }
        yield return null;
    }

    IEnumerator TryUnloadChunks(int chunkIndex)
    {
       // List<int> chunks = new List<int>();
        Vector2 pos = GetChunkCoordinates(chunkIndex);
        KeyValuePair<int, int>[] array;

        if (loadedChunks.Count <= maxLoadedChunks) yield break;
        else
        {
            array = new KeyValuePair<int, int>[loadedChunks.Count - maxLoadedChunks];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new KeyValuePair<int, int>(-1,int.MaxValue);
            }

            foreach (var item in loadedChunks)
            {
                Vector2 chunkPos = GetChunkCoordinates(item.Key);
                if (math.abs(chunkPos.x - pos.x) > renderChunks || math.abs(chunkPos.y - pos.y) > renderChunks)
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i].Value > item.Value.loadedTime)
                        {
                            array[i] = new KeyValuePair<int, int>(item.Key, item.Value.loadedTime);
                            break;
                        }
                    }
                }
            }
        }

       

        //chunks.Add(item.Key);

        for (int i = 0; i < array.Length; i++)
        {
            StartCoroutine(UnloadChunk(array[i].Key));
            loadedChunks.Remove(array[i].Key);
            yield return null;
        }
        yield return null;
    }
    IEnumerator UnloadChunk(int index)
    {
        if(!loadedChunks.ContainsKey(index)) yield break;
        if(loadedChunks[index] != null) Destroy(loadedChunks[index].transform.gameObject);
        Chunk chunk = map.chunks[index];
        var grid = chunk.grid;

        for (int x = 0; x < map.chunkSize; x++)
        {
            for (int y = 0; y < map.chunkSize; y++)
            {
                var value = grid[x, y].gridObject;
                if (value != null && value.objectTransform != null)
                {
                    Destroy(value.objectTransform.gameObject);
                }
            }
        }

        for (int i = 0; i < chunk.items.Count; i++)
        {
            var item = chunk.items[i];
            if (item != null && item.worldItem != null)
            {
                UnloadWorldItem(item);
            }
        }
        yield return null;

    }
    private bool CheckChunk(int chunkIndex)
    {
        if(!loadedChunks.ContainsKey(chunkIndex) && chunkIndex >= 0 && chunkIndex < map.chunkCount)
        {
            return true;
        }
        return false;
    }
    IEnumerator LoadChunk(int chunkIndex)
    {
        if (loadedChunks.ContainsKey(chunkIndex)) yield break;
        Chunk chunk = map.chunks[chunkIndex];
        loadedChunks.Add(chunkIndex, new LoadedChunk((int)Time.time, CreateMesh(chunk)));
        for (int x = 0; x < map.chunkSize; x++)
        {
            for (int y = 0; y < map.chunkSize; y++)
            {
                var value = chunk.grid[x, y].gridObject;
                if (value != null)
                {
                    BuildingManager.instance.LoadObject(value,chunk.ChunkGridPosition + new Vector2(x,y));
                }

            }
        }

        for (int x = 0; x < map.chunkSize; x++)
        {
            for (int y = 0; y < map.chunkSize; y++)
            {
                var value = chunk.grid[x, y].gridObject;
                if(value != null &&  value is GridWall)
                {
                    SetNewSprite(GetCoordinatesByLocalChunkCoordinates(chunkIndex,x,y),value.ID);
                }
            }
        }

        for (int i = 0; i < chunk.items.Count; i++)
        {
            var value = chunk.items[i];
            if(value != null)
            {
                LoadWorldItem(i,chunk);
            }
        }
        yield return null;
    }
    public int GetChunkIndexByPositionXY(Vector2 position)
    {
        return (int)position.x / map.chunkSize + ((int)position.y/ map.chunkSize) * map.widthInChunks;
    }
    public int GetChunkIndexByCoordinates(Vector2 coordinates)
    {
        if(coordinates.x >= 0 && coordinates.y >= 0 && coordinates.x < map.widthInChunks && coordinates.y < map.heightInChunks)
        {
           return (int)coordinates.x   + (int)coordinates.y * map.widthInChunks;
        }
        else
        {
            return -1;
        }
    }

    public GridTile GetGridTileByPositionXY(Vector2 position)
    {
        return GetGridTileByPositionXY((int)position.x,(int)position.y);
    }

    public GridTile GetGridTileByPositionXY(int x,int y)
    {
        if (x >= 0 && y >= 0 && x < map.width && y < map.height)
        {
            int chunkIndex = GetChunkIndexByPositionXY(new Vector2(x, y));
            return map.chunks[chunkIndex].grid[x % map.chunkSize, y % map.chunkSize];
        }
        return null;
    }

    public GridTile GetGridTileByPositionXY(int x, int y,out int chunkIndex)
    {
        if (x >= 0 && y >= 0 && x < map.mapWidthOnWorldScale & y < map.mapHeightOnWorldScale)
        {
            chunkIndex = GetChunkIndexByPositionXY(new Vector2(x, y));
            return map.chunks[chunkIndex].grid[x % map.chunkSize, y % map.chunkSize];
        }
        chunkIndex = -1;
        return null;
    }
    public int GetChunkIndexByWorldPosition(Vector2 worldPosition)
    {
        if (worldPosition.x >= 0 && worldPosition.y >= 0 && worldPosition.x < map.mapWidthOnWorldScale && worldPosition.y < map.mapHeightOnWorldScale)
        {
            return (int)(worldPosition.x / map.chunkSizeOnWorldScale) + (int)(worldPosition.y / map.chunkSizeOnWorldScale) * map.widthInChunks;
        }
        else
        {
            return -1;
        }
    }

    private void UVSet(Vector2[] uv,int index, Vector2 uv00,Vector2 uv11)
    {
        uv[index * 4]     = new Vector2(uv00.x + width1,uv00.y + height1);
        uv[index * 4 + 1] = new Vector2(uv00.x + width1,uv11.y          );
        uv[index * 4 + 2] = new Vector2(uv11.x         ,uv11.y          );
        uv[index * 4 + 3] = new Vector2(uv11.x         ,uv00.y + height1);
    }

    public void UpdateMesh(int x,int y,bool repeat)
    {
        int chunkIndex = GetChunkIndexByPositionXY(new Vector2(x, y));
        if (x >= 0 && y >= 0 && x < map.width && y < map.height && loadedChunks.ContainsKey(chunkIndex))
        {
            int localX = x % map.chunkSize;
            int localY = y % map.chunkSize;

            Mesh mesh = loadedChunks[chunkIndex].transform.GetComponent<MeshFilter>().mesh;
            Mesh lineMesh = loadedChunks[chunkIndex].transform.GetChild(0).GetComponent<MeshFilter>().mesh;
            Vector2[] uv = mesh.uv;
            Vector2[] linesUv = lineMesh.uv;

            int index = localX + localY * map.chunkSize;
            GridTile gridTile = map.chunks[chunkIndex].grid[localX,localY];

            Vector2 uv11, uv00;
            int borders = CalculateBorders(x, y, gridTile.tileID);

            if (borders != gridTile.borders || gridTile.GridObjectIsType<GridHole>() || repeat)
            {
                gridTile.borders = borders;
                if (gridTile.GridObjectIsType<GridHole>(out GridHole hole))
                {
                    GridTile tile = GetTileByGridPosition(x, y + 1);
                    if (tile != null && tile.GridObjectIsType<GridHole>())
                        GetUVTile(gridTile, 1 + hole.waterLevel * 2, out uv00, out uv11);
                    else
                        GetUVTile(gridTile, 0 + hole.waterLevel * 2, out uv00, out uv11);
                }
                else
                    GetUVTile(gridTile, out uv00, out uv11);

                UVSet(uv, index, uv00, uv11); 
                mesh.uv = uv;

                GetUVLine(borders, out uv00, out uv11);
                UVSet(linesUv, index, uv00, uv11);
                lineMesh.uv = linesUv;
            }

            if (repeat)
            {
                UpdateMesh(x + 1, y,false);
                UpdateMesh(x - 1, y,false);
                UpdateMesh(x, y + 1,false);
                UpdateMesh(x, y - 1,false);

                UpdateMesh(x + 1, y + 1,false);
                UpdateMesh(x - 1, y + 1,false);
                UpdateMesh(x - 1, y - 1,false);
                UpdateMesh(x + 1, y - 1,false);
            }
        }
    }

    public Vector2 GetCoordinatesByLocalChunkCoordinates(int chunkIndex,Vector2 localCoordinates)
    {
        Vector2 pos = GetChunkCoordinates(chunkIndex);
        return new Vector2(pos.x * map.chunkSize + localCoordinates.x, pos.y * map.chunkSize + localCoordinates.y);
    }

    public Vector2 GetCoordinatesByLocalChunkCoordinates(int chunkIndex, int x ,int y)
    {
        return GetCoordinatesByLocalChunkCoordinates(chunkIndex, new Vector2(x, y));
    }

    private void GetUVTile(GridTile gridTile,out Vector2 uv00, out Vector2 uv11)
    {
        TileUV tileUV = TilesUV[gridTile.tileID];
        uv00 = tileUV.uv00 + (new Vector2(tileWidth, 0) * gridTile.variant);
        uv11 = (tileUV.uv00 + new Vector2(tileWidth, tileHeight)) + (new Vector2(tileWidth, 0) * gridTile.variant);


        //if (gridTile.borders == 0 || tileUV.uv00Grass == null)
        //{
        //    uv00 = tileUV.uv00 + (new Vector2(tileWidth, 0) * gridTile.variant);
        //    uv11 = (tileUV.uv00 + new Vector2(tileWidth, tileHeight)) + (new Vector2(tileWidth, 0) * gridTile.variant);
        //}
        //else
        //{
        //    if(gridTile.borders < 0) gridTile.borders = 15 - gridTile.borders;
        //    gridTile.borders--;

        //    uv00 = (Vector2)tileUV.uv00Grass + new Vector2(tileWidth, 0) * gridTile.borders;
        //    uv11 = (Vector2)tileUV.uv00Grass + new Vector2(tileWidth,tileHeight) + new Vector2(tileWidth, 0) * gridTile.borders;
        //}    
    }
    private void GetUVTile(GridTile gridTile,int variant, out Vector2 uv00, out Vector2 uv11)
    {
        GetUVTile(gridTile.tileID,variant, out uv00, out uv11);
    }
    private void GetUVTile(int tileID, int variant, out Vector2 uv00, out Vector2 uv11)
    {
        TileUV tileUV = TilesUV[tileID];
        uv00 = tileUV.uv00 + (new Vector2(tileWidth, 0) * variant);
        uv11 = (tileUV.uv00 + new Vector2(tileWidth, tileHeight)) + (new Vector2(tileWidth, 0) * variant);
    }
    private void GetUVLine(int variant, out Vector2 uv00, out Vector2 uv11)
    {
        TileUV tileUV = TilesUV[-1];
        uv00 = tileUV.uv00 + (new Vector2(linetileWidth, 0) * variant);
        uv11 = (tileUV.uv00 + new Vector2(linetileWidth, 1)) + (new Vector2(linetileWidth, 0) * variant);
    }

    private int CalculateBorders(int x, int y,int id)
    {
        int value = 0;
        if (GetTileByGridPosition(x, y + 1)?.tileID > id) value += 1;
        if (GetTileByGridPosition(x + 1, y)?.tileID > id) value += 2;
        if (GetTileByGridPosition(x, y - 1)?.tileID > id) value += 4; 
        if (GetTileByGridPosition(x - 1, y)?.tileID > id) value += 8; 
        return value;
    }

    //private int CalculateBorders(int x,int y)
    //{
    //    int value = 0;
    //    int number = 0;


    //    if (GetValueByGridPosition(x, y + 1)?.tileID  == settings.grassID) { value += 1; number++; }
    //    if (GetValueByGridPosition(x + 1, y )?.tileID == settings.grassID) { value += 2; number++; }
    //    if (GetValueByGridPosition(x , y - 1)?.tileID == settings.grassID) { value += 4; number++; }
    //    if (GetValueByGridPosition(x - 1, y )?.tileID == settings.grassID) { value += 8; number++; }

    //    if (value == 0)
    //    {
    //        if (GetValueByGridPosition(x + 1, y + 1)?.tileID == settings.grassID) value -= 1;
    //        if (GetValueByGridPosition(x + 1, y - 1)?.tileID == settings.grassID) value -= 2;
    //        if (GetValueByGridPosition(x - 1, y - 1)?.tileID == settings.grassID) value -= 4;
    //        if (GetValueByGridPosition(x - 1, y + 1)?.tileID == settings.grassID) value -= 8;
    //    }
    //    else if(number == 1)
    //    {
    //        int k = 0;
    //        switch (value)
    //        {
    //            case 1:
    //                if (GetValueByGridPosition(x - 1, y - 1)?.tileID == settings.grassID) k += 1;
    //                if (GetValueByGridPosition(x + 1, y - 1)?.tileID == settings.grassID) k += 2;
    //                break;
    //            case 2:
    //                if (GetValueByGridPosition(x - 1, y - 1)?.tileID == settings.grassID) k += 1;
    //                if (GetValueByGridPosition(x - 1, y + 1)?.tileID == settings.grassID) k += 2;
    //                break;
    //            case 4:
    //                if (GetValueByGridPosition(x - 1, y + 1)?.tileID == settings.grassID) k += 1;
    //                if (GetValueByGridPosition(x + 1, y + 1)?.tileID == settings.grassID) k += 2;
    //                break;
    //            case 8:
    //                if (GetValueByGridPosition(x + 1, y - 1)?.tileID == settings.grassID) k += 1;
    //                if (GetValueByGridPosition(x + 1, y + 1)?.tileID == settings.grassID) k += 2;
    //                break;
    //        }
    //        if (k > 0)
    //        {
    //            if(value != 8) value = 30 + ((value / 2) * 3) + k;
    //            else value = 39 + k;
    //        }
    //    }
    //    else if (number == 2)
    //    {      
    //        switch (value)
    //        {
    //            case 3:
    //                if (GetValueByGridPosition(x - 1, y - 1)?.tileID == settings.grassID) value = 43;
    //                break;
    //            case 6:
    //                if (GetValueByGridPosition(x - 1, y + 1)?.tileID == settings.grassID) value = 44;
    //                break;
    //            case 9:
    //                if (GetValueByGridPosition(x + 1, y - 1)?.tileID == settings.grassID) value = 45;
    //                break;
    //            case 12:
    //                if (GetValueByGridPosition(x + 1, y + 1)?.tileID == settings.grassID) value = 46;
    //                break;
    //        }
    //    }

    //    return value;
    //}



    public Transform CreateMesh(Chunk chunk)
    {
        Transform partOfMap = new GameObject("part of map").transform;
        Transform lines = new GameObject("Lines").transform;
        lines.SetParent(partOfMap);

        MeshFilter meshFilter = partOfMap.AddComponent<MeshFilter>();
        MeshFilter linesMeshFilter = lines.AddComponent<MeshFilter>();

        meshFilter.AddComponent<SortingGroup>().sortingOrder = -10;
        linesMeshFilter.AddComponent<SortingGroup>().sortingOrder = 0;
        int width = map.chunkSize;
        int height = map.chunkSize;
        float cellSize = map.cellSize;
        
        Mesh mesh = new Mesh();
        Mesh linesMesh= new Mesh();

        meshFilter.transform.position = new Vector3(chunk.position.x, chunk.position.y, 10);

        Vector3[] vertices = new Vector3[4 * (width * height)];
        int[] triangles = new int[6 * (width * height)];

        Vector2[] uv = new Vector2[4 * (width * height)];
        Vector2[] linesUV = new Vector2[4 * (width * height)];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = x + y * width;
                vertices[index * 4 + 0] = new Vector3( x * cellSize,       y * cellSize);
                vertices[index * 4 + 1] = new Vector3( x * cellSize,      (y + 1) * cellSize);
                vertices[index * 4 + 2] = new Vector3((x + 1) * cellSize, (y + 1) * cellSize);
                vertices[index * 4 + 3] = new Vector3((x + 1) * cellSize,  y * cellSize);

                triangles[index * 6]     = index * 4;
                triangles[index * 6 + 1] = index * 4 + 1;
                triangles[index * 6 + 2] = index * 4 + 2;

                triangles[index * 6 + 3] = index * 4; 
                triangles[index * 6 + 4] = index * 4 + 2;
                triangles[index * 6 + 5] = index * 4 + 3;

                GridTile gridTile = chunk.grid[x, y];
                int borders = CalculateBorders(x, y, gridTile.tileID);

                Vector2 uv11, uv00;


                if (gridTile.GridObjectIsType<GridHole>(out GridHole hole))
                {
                    GridTile tile = GetTileByGridPosition(gridTile.x, gridTile.y + 1);
                    if (tile != null && tile.GridObjectIsType<GridHole>())
                    {
                        GetUVTile(gridTile, 1 + hole.waterLevel * 2, out uv00, out uv11);
                    }
                    else
                    {
                        GetUVTile(gridTile, 0 + hole.waterLevel * 2, out uv00, out uv11);
                    }
                }
                else
                    GetUVTile(gridTile, out uv00, out uv11);
                

                UVSet(uv, index, uv00, uv11);

                borders = CalculateBorders(x + (int)chunk.ChunkGridPosition.x, y + (int)chunk.ChunkGridPosition.y, gridTile.tileID);
                GetUVLine(borders, out uv00, out uv11);
                UVSet(linesUV, index, uv00, uv11);
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        linesMesh.vertices = vertices;
        linesMesh.uv = linesUV;
        linesMesh.triangles = triangles;

        MeshRenderer meshRenderer = meshFilter.AddComponent<MeshRenderer>();
        meshRenderer.material = mapMaterial;
        meshRenderer.material.mainTexture = mapTexture;

        MeshRenderer linesMeshRenderer = linesMeshFilter.AddComponent<MeshRenderer>();
        linesMeshRenderer.material = mapMaterial;
        linesMeshRenderer.material.mainTexture = linesTexture;
        linesMeshFilter.mesh = linesMesh;

        meshFilter.transform.parent = transform;
        meshFilter.mesh = mesh;
        return meshFilter.transform;
    }
    private bool IsGrass(int tileID)
    {
       return TilesUV.ContainsKey(tileID) && TilesUV[tileID].uv00Grass != null;
    }
    public void PlayerMovement(Vector2 worldPosition)
    {
        CheckChunks(worldPosition);
        onPlayerMove?.Invoke(this, new PlayerPositionArgs(GetGridPosition(worldPosition), lastPlayerChunk, GetChunkCoordinates(lastPlayerChunk)));
    }
    public Vector2 GetChunkCoordinates(int chunk)
    {
        int x = chunk % map.widthInChunks;
        int y = chunk / map.widthInChunks;
        return new Vector2(x, y);
    }
    public Vector2 GetGridPosition(Vector2 position)
    {
        int x = Mathf.FloorToInt((position.x - map.offset.x) / map.cellSize);
        int y = Mathf.FloorToInt((position.y - map.offset.y) / map.cellSize);
        return new Vector2(x, y);
    }
    public void TileChanged(int x,int y)
    {
        UpdateMesh(x,y,true);
    }
    public GridTile GetTileByGridPosition(Vector2 gridPosition)
    {
        int chunkIndex =  GetChunkIndexByPositionXY(gridPosition);
        // loadedChunks.ContainsKey(chunkIndex)
        if (map.chunks.ContainsKey(chunkIndex) && gridPosition.x >= 0 && gridPosition.y >= 0 && gridPosition.x < map.width && gridPosition.y < map.height)
        {
            return map.chunks[chunkIndex].grid[(int)gridPosition.x % map.chunkSize, (int)gridPosition.y % map.chunkSize];
        }
        return null;
    }
    public GridTile GetTileByGridPosition(int x,int y)
    {
        return GetTileByGridPosition(new Vector2(x, y));
    }
    public Vector2 GetWorldPosition(Vector2 gridPosition)
    {
        return map.offset + gridPosition * map.cellSize + new Vector2(map.cellSize / 2, 0);
    }
    public Vector2 GetWorldPosition(int x, int y)
    {
        return GetWorldPosition(new Vector2(x, y)); 
    }

    public GridTile[,] GetGridByXY(Vector2 posXY)
    {
        int x = (int)posXY.x % map.chunkSize;
        int y = (int)posXY.y % map.chunkSize;
        return map.chunks[x + y * map.widthInChunks].grid;
    }
    public void DestroyObject(GridTile gridTile,bool drop)
    {
        int id = gridTile.gridObject.ID;
        Item item = ItemsAsset.instance.GetItem(id);
        Vector2 vector2 = new Vector2(gridTile.x, gridTile.y);
        Destroy(gridTile.gridObject.objectTransform.gameObject);
        if(drop) 
            DestroyDrop(gridTile.gridObject, vector2);

        if (gridTile.gridObject.variantIndex >= 0)
        {
            Variant variant = ((VariantItem)item).objectVariants[gridTile.gridObject.variantIndex].variants[gridTile.gridObject.stateIndex];
            Vector2 mainPos = gridTile.gridObject.mainPosition;
            GetTileByGridPosition(mainPos)?.SetGridObject(null);
            for (int i = 0; i < variant.objectPoints.Length; i++)
            {
                GetTileByGridPosition(mainPos + variant.objectPoints[i])?.SetGridObject(null);
            }
        }
        if (item is WallObject) UpdateNeighbors(vector2, id); 
    }
    public void DestroySurface(GridTile ground)
    {
        ground.SetGridObject(null,true);
    }
    public void DestroyDrop(GridObject gridObject, Vector2 pos)
    {
        BuildingItem item = (BuildingItem)ItemsAsset.instance.GetItem(gridObject.ID);
        if(gridObject is GridContainer)
        {
            GridContainer container = (GridContainer)gridObject;
            for (int i = 0;i  < container.items.Length ;i++)
            {
                if (container.items[i] != null)
                {
                    Vector2 target = GetWorldPosition(pos + new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(0f, 0.5f)));
                    CreateWorldItem(new ItemStats(container.items[i]), GetWorldPosition(pos + new Vector2(0, 0.5f)), target);
                }
            }
        }

        for (int i = 0; i < item.drop.Length; i++)
        {
            Drop drop = item.drop[i];
            if(drop.probability > 0 && drop.probability >= UnityEngine.Random.Range(0f,1f))
            {
                int number = drop.ingredient.number;
                if(drop.maxNumber > 0) number = UnityEngine.Random.Range(drop.ingredient.number, drop.maxNumber + 1);
                for (int j = 0; j < number; j++)
                {
                    Vector2 target = GetWorldPosition(pos + new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(0f, 0.5f)));
                    CreateWorldItem(new ItemStats(drop.ingredient.itemID), GetWorldPosition(pos + new Vector2(0, 0.5f)), target);
                }
            }
        }

        if(item.drop.Length == 0)
        {
            Vector2 target = GetWorldPosition(pos + new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(0f, 0.5f)));
            CreateWorldItem(new ItemStats(gridObject.ID), GetWorldPosition(pos + new Vector2(0, 0.5f)), target);
        }

    }
    public void UpdateNeighbors(Vector2 positionXY, int ID)
    {
        bool[] neighbors = GetNeighbors(positionXY, ID);
        for (int i = 0; i < 4; i++)
        {
            if (neighbors[i]) UpdateSprite(positionXY + MyTools.directions4[i]);
        }
    }
    private bool[] GetNeighbors(Vector2 positionXY, int ID)
    {
        bool[] neighbors = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            var obj = GetTileByGridPosition(positionXY + MyTools.directions4[i]);
            if (obj != null && obj.IsBuildObject(ID)) neighbors[i] = true;
            else neighbors[i] = false;
        }
        return neighbors;
    }
    private void UpdateSprite(Vector2 positionXY)
    {
        var gridTile = GetTileByGridPosition(positionXY);
        if (gridTile == null) return;
        GridObject gridObject = gridTile.gridObject;
        if (gridObject == null) return;
        bool[] neighbors = GetNeighbors(positionXY, gridObject.ID);
        int value = 0;

        for (int i = 0; i < neighbors.Length; i++)
        {
            if (neighbors[i])
            {
                value += (int)math.pow(2f, i);
            }
        }

        if (neighbors[2])
        {
            gridObject.objectTransform.GetComponent<SortingGroup>().sortingOrder = 1;
        }
        else
        {
            gridObject.objectTransform.GetComponent<SortingGroup>().sortingOrder = 0;
        }

        Transform child = gridObject.objectTransform.GetChild(0);
        ObjectVariant objectVariant = ItemsAsset.instance.GetObjectVariant(gridObject.ID, value);
        child.GetComponent<SpriteRenderer>().sprite = objectVariant.variants[0].sprite;
        child.GetComponent<PolygonCollider2D>().points = objectVariant.variants[0].hitbox;
        MyTools.ChangePositionPivot(gridObject.objectTransform, child.TransformPoint(0, objectVariant.variants[0].minY, 0));
    }
    public void SetNewSprite(Vector2 positionXY,int id)
    {
        bool[] neighbors = GetNeighbors(positionXY, id);
        for (int i = 0; i < 4; i++)
        {
            if (neighbors[i]) UpdateSprite(positionXY + MyTools.directions4[i]);
        }
        UpdateSprite(positionXY);
    }
    public void CreateWorldItem(ItemStats item,Vector2 pos,Vector2 target)
    {
        int chunkIndex = GetChunkIndexByWorldPosition(new Vector2(target.x, target.y));
        Transform itemTransform = Instantiate(worldItem, pos, Quaternion.identity,transform).transform;
        Vector2 vector2 = GetGridPosition(target);
        ChunkItem newItem = new ChunkItem(item, vector2, itemTransform);
        int itemChunkIndex = map.chunks[chunkIndex].AddItem(newItem);

        WorldItem witem = itemTransform.GetComponent<WorldItem>();

        witem.SetItem(item, target, itemChunkIndex,chunkIndex);

        if (item.itemCount < ItemsAsset.instance.GetStackMax(item.itemID))
        {
            ChunkItem chunkItem = CheckNeighboringWorldItems(vector2, chunkIndex, itemChunkIndex, item.itemID);
            if (chunkItem != null) witem.AddStacks(chunkItem, newItem,false);
        }
    }

    public void StartAddStacks(Vector2 worldPosition,int chunkIndex, WorldItem worldItem)
    {
        Vector2 vector2 = GetGridPosition(worldPosition);
        ChunkItem oldchunkItem = map.chunks[chunkIndex].items[worldItem.itemChunkIndex];

        if (worldItem.itemStats.itemCount < ItemsAsset.instance.GetStackMax(worldItem.itemStats.itemID))
        {
            ChunkItem chunkItem = CheckNeighboringWorldItems(vector2, chunkIndex, worldItem.itemChunkIndex, worldItem.itemStats.itemID);
            if (chunkItem != null)
            {
                worldItem.AddStacks(chunkItem, oldchunkItem,true);
            }
        }
    }
    public bool AddStacks(ChunkItem chunkItem,ItemStats itemStats)
    {
        int free = ItemsAsset.instance.GetStackMax(chunkItem.item.itemID) - chunkItem.item.itemCount;
        if(itemStats.itemCount - free > 0)
        {
            chunkItem.item.itemCount += free;
            itemStats.itemCount -= free;
            return false;
        }
        else
        {
            chunkItem.item.itemCount += itemStats.itemCount;
            return true;
        }
    }
    public ChunkItem CheckNeighboringWorldItems(Vector2 posXY, int chunkIndex, int itemChunkIndex, int id)
    {
        ChunkItem chunkItem;
        chunkItem = map.chunks[chunkIndex].FindItem(posXY,id, itemChunkIndex);
        if (chunkItem != null) return chunkItem;
        bool[] isFreeTile = CheckNeighboringFreeTile(posXY);

        for (int i = 0; i < 8; i++)
        {
            if (i % 2 == 1)
            {
                if (isFreeTile[(i - 1)/2] || isFreeTile[((i + 1) / 2)%4])
                {
                    int newChunkIndex = GetChunkIndexByPositionXY(posXY + MyTools.directions8[i]);
                    chunkItem = map.chunks[newChunkIndex].FindItem(posXY + MyTools.directions8[i], id, itemChunkIndex);
                    if (chunkItem != null) return chunkItem;
                }
            }
            else
            {
                int newChunkIndex = GetChunkIndexByPositionXY(posXY + MyTools.directions8[i]);
                chunkItem = map.chunks[newChunkIndex].FindItem(posXY + MyTools.directions8[i], id, itemChunkIndex);
                if (chunkItem != null) return chunkItem;
            }
        }
        return null;
    }

    private bool[] CheckNeighboringFreeTile(Vector2 posXY)
    {
        bool[] isFreeTile = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            GridTile gridTile = GetTileByGridPosition(posXY + MyTools.directions4[i]);
            if (gridTile != null) isFreeTile[i] = gridTile.isWalkable;
            else isFreeTile[i] = true;
        }
        return isFreeTile;
    }

    public void MoveWorldItems(Vector2 posXY)
    {
        int chunkIndex = GetChunkIndexByPositionXY(posXY);
        Vector2? newPosXY = null;

        bool[] isFreeTile = new bool[8];
        for (int i = 0; i < 8; i++)
        {
            GridTile gridTile = GetTileByGridPosition(posXY + MyTools.directions8[i]);
            if (gridTile != null) isFreeTile[i] = gridTile.isWalkable;
            else isFreeTile[i] = false; 
        }

        for (int i = 0; i < 8; i++)
        {
            if (isFreeTile[i])
            {
                if (i % 2 == 1)
                {
                    if (isFreeTile[i - 1] || isFreeTile[(i + 1) % 8])
                    {
                        newPosXY = posXY + MyTools.directions8[i];
                    }
                }
                else
                {
                    newPosXY = posXY + MyTools.directions8[i];
                    break;
                }
            }
        }

        if (newPosXY != null)
        {
            int newChunkIndex = GetChunkIndexByPositionXY((Vector2)newPosXY);
            map.chunks[chunkIndex].MoveAllItems(posXY, GetWorldPosition((Vector2)newPosXY + new Vector2(0,0.5f)),chunkIndex,newChunkIndex);
        }
        else
        {
            map.chunks[chunkIndex].RemoveAllItems(posXY);
        }

    }
    public void CreateWorldItem(ItemStats item,Vector2 posXY)
    {
        Vector2 target = GetWorldPosition(posXY + new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(0f, 0.5f)));
        CreateWorldItem(item, GetWorldPosition(posXY + new Vector2(0, 0.5f)), target);
    }
    private void LoadWorldItem(int itemChunkIndex,Chunk chunk)
    {
        var item = chunk.items[itemChunkIndex];
        Vector2 pos = GetWorldPosition(item.position);

        Transform wItem = Instantiate(worldItem,pos, Quaternion.identity, transform).transform;
        item.worldItem = wItem;
        wItem.GetComponent<WorldItem>().SetItem(item.item, pos, itemChunkIndex,chunk.chunkIndex);
    }
    public void UnloadWorldItem(ChunkItem chunkItem)
    {
        chunkItem.worldItem.GetComponent<WorldItem>().ClearTimers();
        Destroy(chunkItem.worldItem.gameObject);
    }

    public void RemoveWorldItem(int chunkIndex,int itemChunkIndex)
    {
        if (map.chunks.ContainsKey(chunkIndex))
        {
            Chunk chunk = map.chunks[chunkIndex];

            if (itemChunkIndex != -1)
            {
                Transform worldItem = chunk.items[itemChunkIndex].worldItem;
                worldItem.GetComponent<WorldItem>().ClearTimers();
                worldItem.GetComponent<WorldItem>().itemChunkIndex = -1;
                Destroy(worldItem.gameObject);
                chunk.RemoveItem(itemChunkIndex);
            }
        }
        else
            Debug.LogError("GridIndex desn't exist");
    }
    public void RemoveWorldItem(Vector2 worldPosition, int itemChunkIndex)
    {
        int chunkIndex = GetChunkIndexByWorldPosition(new Vector2(worldPosition.x, worldPosition.y));
        RemoveWorldItem(chunkIndex, itemChunkIndex);
    }

    public void PourWater(int water, Vector2 posXY)
    {
        GridTile gridTile = GetGridTileByPositionXY(posXY);
        if (gridTile == null) return;
        GridHole hole;
        if(gridTile.GridObjectIsType<GridHole>(out hole))
        {
            LiquidsManager.instance.WaterTransfer(gridTile, water);
        }
    }

    



    //    GridHole gridHole = null;
    //    List<GridTile> holesToCheck = new List<GridTile>();
    //    List<GridTile> holesToDivideWater = new List<GridTile>();
    //    holesToCheck.Add(gridTile);
    //    holesToDivideWater.Add(gridTile);
    //    gridTile.GridObjectIsType<GridHole>(out gridHole);

    //   // float water = overflow + gridHole.fill;
    //    while (holesToCheck.Count > 0)
    //    {
    //        for (int i = holesToCheck.Count - 1; i >= 0; i--)
    //        {
    //            GridTile hole = holesToCheck[i];
    //            for (int j = 0; j < 4; j++)
    //            {
    //                GridTile tile = GetGridTileByPositionXY(hole.GetXYPosition() + MyTools.directions4[j]);
    //                if (tile != null && tile.GridObjectIsType<GridHole>(out gridHole) && !holesToDivideWater.Contains(tile))
    //                {
    //                    if((water + gridHole.fill) / ((float)holesToDivideWater.Count + 1) < 50) break;
    //                    holesToCheck.Add(tile);
    //                    holesToDivideWater.Add(tile);
    //                    water += gridHole.fill;
    //                }
    //            }
    //            holesToCheck.RemoveAt(i);
    //        }
    //    }

    //    float ration = water / holesToDivideWater.Count;
    //    for (int i = holesToDivideWater.Count - 1; i >= 0; i--)
    //    {
    //        GridTile tile = holesToDivideWater[i];
    //        GridHole hole = tile.gridObject as GridHole;
    //        hole.fill = ration;
    //        UpdateMesh(tile.x, tile.y, true);
    //    }
    //    holesToDivideWater.Clear();

}
