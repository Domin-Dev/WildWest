

using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;


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
    public const int maxLoadedChunks = 40;

    public Map map;
    public Pathfinding pathfinding;
     public Dictionary<int, LoadedChunk> loadedChunks { private set; get; }
    public int lastPlayerChunk { private set; get; } = -1;
    public Vector2 lastPlayerPosition { private set; get; } = Vector2.zero;

    public event EventHandler<PlayerPositionArgs> onPlayerMove;
    public event EventHandler<PlayerPositionArgs> onChangeChunk;

    public Dictionary<int, TileUV> TilesUV { get; private set; }
    public MapGeneratorSettings settings { private set; get; }

    int textureWidth;
    int textureHeight;
    const int sizeTile = 25;//px
    float tileWidth;//    sizeTile /textureWidth;
    float tileHeight;//   sizeTile / textureHeight;
    Texture2D mapTexture;
    float width1;
    float height1;

    public static GridVisualization instance { private set; get; }

    [SerializeField] private Material mapMaterial;
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
        settings = Resources.Load<MapGeneratorSettings>("mapGeneratorSettings");
        SetUpMapMaterial();
    }

    private void SetUpMapMaterial(int sizeTile = 25)
    {
        TilesUV = new Dictionary<int, TileUV>();
        Floor[] array = ItemsAsset.instance.GetItemsByType<Floor>();
        Texture2D texture = new Texture2D(46 * sizeTile, sizeTile * CountTextures(array));
        texture.filterMode = FilterMode.Point;

        textureWidth = texture.width;
        textureHeight = texture.height;
        tileWidth = (float)sizeTile / textureWidth;
        tileHeight = (float)sizeTile / textureHeight;
        width1 = 0.01f / textureWidth;
        height1 = 0.01f / textureHeight;

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
            Vector2 uv00 = new Vector2(0, (float)k * sizeTile / textureHeight);
            CopyTexture(ref k, sizeTile, floor.texture, texture);
            int variants = floor.texture.width / sizeTile;

            TilesUV.Add(floor.ID, new TileUV(uv00, variants, grassUV));
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
        Debug.Log(loadedChunks.Count);
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
        loadedChunks.Add(chunkIndex,new LoadedChunk((int)Time.time,CreateMesh(chunk)));
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
                if(value != null &&  value is Wall)
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

            GridTile[,] grid = map.chunks[chunkIndex].grid;
            Mesh mesh = loadedChunks[chunkIndex].transform.GetComponent<MeshFilter>().mesh;
            Vector2[] uv = mesh.uv;
            int index = localX + localY * map.chunkSize;
            GridTile gridTile = grid[localX,localY];

            Vector2 uv11, uv00;
            int borders = CalculateBorders(x, y);

            if (borders != gridTile.borders || repeat)
            {
                GetUVTile(gridTile.tileID, borders, out uv00, out uv11);
                gridTile.borders = borders;
                UVSet(uv, index, uv00, uv11);
                mesh.uv = uv;
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

    private void GetUVTile(int tileID,int borders,out Vector2 uv00, out Vector2 uv11)
    {
        uv00 = Vector2.zero;
        uv11 = Vector2.zero;

        TileUV tileUV = TilesUV[tileID];
        if (borders == 0 || tileUV.uv00Grass == null)
        {
            if (UnityEngine.Random.Range(1, 101) > 60)
            {
                uv11 = tileUV.uv00 + new Vector2(tileWidth, tileHeight);
                uv00 = tileUV.uv00;
            }
            else
            {
                int variant = 0;
                if(tileUV.variants > 1) variant = UnityEngine.Random.Range(1, tileUV.variants);

                uv00 = tileUV.uv00 + (new Vector2(tileWidth, 0) * variant);
                uv11 = (tileUV.uv00 + new Vector2(tileWidth, tileHeight)) + (new Vector2(tileWidth, 0) * variant);
            }
        }
        else
        {
            if(borders < 0) borders = 15 - borders;
            borders--;

            uv00 = (Vector2)tileUV.uv00Grass + new Vector2(tileWidth, 0) * borders;
            uv11 = (Vector2)tileUV.uv00Grass + new Vector2(tileWidth,tileHeight) + new Vector2(tileWidth, 0) * borders;
        }  
        
    }
    private int CalculateBorders(int x,int y)
    {
        int value = 0;
        int number = 0;

        if (GetValueByGridPosition(x, y + 1)?.tileID  == settings.grassID) { value += 1; number++; }
        if (GetValueByGridPosition(x + 1, y )?.tileID == settings.grassID) { value += 2; number++; }
        if (GetValueByGridPosition(x , y - 1)?.tileID == settings.grassID) { value += 4; number++; }
        if (GetValueByGridPosition(x - 1, y )?.tileID == settings.grassID) { value += 8; number++; }

        if (value == 0)
        {
            if (GetValueByGridPosition(x + 1, y + 1)?.tileID == settings.grassID) value -= 1;
            if (GetValueByGridPosition(x + 1, y - 1)?.tileID == settings.grassID) value -= 2;
            if (GetValueByGridPosition(x - 1, y - 1)?.tileID == settings.grassID) value -= 4;
            if (GetValueByGridPosition(x - 1, y + 1)?.tileID == settings.grassID) value -= 8;
        }
        else if(number == 1)
        {
            int k = 0;
            switch (value)
            {
                case 1:
                    if (GetValueByGridPosition(x - 1, y - 1)?.tileID == settings.grassID) k += 1;
                    if (GetValueByGridPosition(x + 1, y - 1)?.tileID == settings.grassID) k += 2;
                    break;
                case 2:
                    if (GetValueByGridPosition(x - 1, y - 1)?.tileID == settings.grassID) k += 1;
                    if (GetValueByGridPosition(x - 1, y + 1)?.tileID == settings.grassID) k += 2;
                    break;
                case 4:
                    if (GetValueByGridPosition(x - 1, y + 1)?.tileID == settings.grassID) k += 1;
                    if (GetValueByGridPosition(x + 1, y + 1)?.tileID == settings.grassID) k += 2;
                    break;
                case 8:
                    if (GetValueByGridPosition(x + 1, y - 1)?.tileID == settings.grassID) k += 1;
                    if (GetValueByGridPosition(x + 1, y + 1)?.tileID == settings.grassID) k += 2;
                    break;
            }
            if (k > 0)
            {
                if(value != 8) value = 30 + ((value / 2) * 3) + k;
                else value = 39 + k;
            }
        }
        else if (number == 2)
        {      
            switch (value)
            {
                case 3:
                    if (GetValueByGridPosition(x - 1, y - 1)?.tileID == settings.grassID) value = 43;
                    break;
                case 6:
                    if (GetValueByGridPosition(x - 1, y + 1)?.tileID == settings.grassID) value = 44;
                    break;
                case 9:
                    if (GetValueByGridPosition(x + 1, y - 1)?.tileID == settings.grassID) value = 45;
                    break;
                case 12:
                    if (GetValueByGridPosition(x + 1, y + 1)?.tileID == settings.grassID) value = 46;
                    break;
            }
        }

        return value;
    }
    public Transform CreateMesh(Chunk chunk)
    {
        MeshFilter meshFilter = new GameObject("part of map").AddComponent<MeshFilter>();
        meshFilter.AddComponent<SortingGroup>().sortingOrder = -10;
        int width = map.chunkSize;
        int height = map.chunkSize;
        float cellSize = map.cellSize;
        
        Mesh mesh = new Mesh();

        meshFilter.transform.position = new Vector3(chunk.position.x, chunk.position.y, 10);
        Vector3[] vertices = new Vector3[4 * (width * height)];
        Vector2[] uv = new Vector2[4 * (width * height)];
        int[] triangles = new int[6 * (width * height)];

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
             
                int borders = 0;
                if (IsGrass(gridTile.tileID)) borders = CalculateBorders(x + (int)chunk.ChunkGridPosition.x, y + (int)chunk.ChunkGridPosition.y);
                Vector2 uv11, uv00;
                GetUVTile(gridTile.tileID, borders, out uv00, out uv11);
                gridTile.borders = borders;
                UVSet(uv, index, uv00, uv11);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        MeshRenderer  meshRenderer = meshFilter.AddComponent<MeshRenderer>();
        meshRenderer.material = mapMaterial;
        meshRenderer.material.mainTexture = mapTexture;
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
    public GridTile GetValueByGridPosition(Vector2 gridPosition)
    {
        int chunkIndex =  GetChunkIndexByPositionXY(gridPosition);
        // loadedChunks.ContainsKey(chunkIndex)
        if (map.chunks.ContainsKey(chunkIndex) && gridPosition.x >= 0 && gridPosition.y >= 0 && gridPosition.x < map.width && gridPosition.y < map.height)
        {
            return map.chunks[chunkIndex].grid[(int)gridPosition.x % map.chunkSize, (int)gridPosition.y % map.chunkSize];
        }
        return null;
    }
    public GridTile GetValueByGridPosition(int x,int y)
    {
        return GetValueByGridPosition(new Vector2(x, y));
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
    public void DestroyObject(GridTile gridTile)
    {
        int id = gridTile.gridObject.ID;
        Item item = ItemsAsset.instance.GetItem(id);
        Vector2 vector2 = new Vector2(gridTile.x, gridTile.y);
        Destroy(gridTile.gridObject.objectTransform.gameObject);
        DestroyDrop(gridTile.gridObject, vector2);

        gridTile.SetGridObject(null);
        if (item is WallObject) UpdateNeighbors(vector2, id); 
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
            var obj = GetValueByGridPosition(positionXY + MyTools.directions4[i]);
            if (obj != null && obj.IsBuildObject(ID)) neighbors[i] = true;
            else neighbors[i] = false;
        }
        return neighbors;
    }
    private void UpdateSprite(Vector2 positionXY)
    {
        var gridTile = GetValueByGridPosition(positionXY);
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
        Debug.Log(objectVariant);
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
        int gridIndex = GetChunkIndexByWorldPosition(new Vector2(target.x, target.y));
        Transform wItem = Instantiate(worldItem, pos, Quaternion.identity,transform).transform;
        int itemChunkIndex = map.chunks[gridIndex].AddItem(new ChunkItem(item, target,wItem));
        wItem.GetComponent<WorldItem>().SetItem(item, target, itemChunkIndex);
    }
    private void LoadWorldItem(int itemChunkIndex,Chunk chunk)
    {
        var item = chunk.items[itemChunkIndex];
        Transform wItem = Instantiate(worldItem, item.position, Quaternion.identity, transform).transform;
        item.worldItem = wItem;
        wItem.GetComponent<WorldItem>().SetItem(item.item, item.position, itemChunkIndex);
    }
    public void UnloadWorldItem(ChunkItem chunkItem)
    {
        chunkItem.worldItem.GetComponent<WorldItem>().ClearTimers();
        Destroy(chunkItem.worldItem.gameObject);
    }
    public void RemoveWorldItem(Vector2 pos,int itemChunkIndex) 
    {
        int gridIndex = GetChunkIndexByWorldPosition(new Vector2(pos.x, pos.y));
        Chunk chunk = map.chunks[gridIndex];
        Transform worldItem = chunk.items[itemChunkIndex].worldItem;
        worldItem.GetComponent<WorldItem>().ClearTimers();
        Destroy(worldItem.gameObject);
        chunk.RemoveItem(itemChunkIndex);
    }
}
