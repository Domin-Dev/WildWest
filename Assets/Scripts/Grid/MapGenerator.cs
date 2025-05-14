using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


public class MapGenerator 
{
    private int seed;
    [Header("Map Size( in chunks )")]
    private int widthInChunks = 10;
    private int heightInChunks = 10;
    [Header("Map Generator Settings")]
    [SerializeField] private float scale = 20;
    [SerializeField] private Vector2 offset;
    [Header("Temperature Map Settings")]
    [SerializeField] private float scaleTemp = 20;
    [SerializeField] private Vector2 offsetTemp;
    [Header("Rainfall Map Settings")]
    [SerializeField] private float scaleRain = 20;
    [SerializeField] private Vector2 offsetRain;
    [Header("Height Settings")]
    [SerializeField] private Vector2 scaleHeight;
    [SerializeField] private Vector2 offsetHeight;
    [Header("Chunk Settings")]
    [SerializeField] private int chunkSize = 10;

    private static readonly Vector2 gridOffset = new Vector2(0,0); 

    private MapGeneratorSettings mapGeneratorSettings;

    public MapGenerator(int seed)
    {
        this.seed = seed;
        mapGeneratorSettings = Resources.Load<MapGeneratorSettings>("mapGeneratorSettings");
        Debug.Log(mapGeneratorSettings);
    }
    public Map StartGenerator()
    {
        seed = GameInfo.instance.seed;

        var rand = new System.Random(seed);
        offset.x = rand.Next(-100000, 100000);
        offset.y = rand.Next(-100000, 100000);

        offsetTemp.x = rand.Next(-100000, 100000);
        offsetTemp.y = rand.Next(-100000, 100000);

        offsetRain.x = rand.Next(-100000, 100000);
        offsetRain.y = rand.Next(-100000, 100000);

        GenerateMap(0.25f, gridOffset,out Map map);

        return map;
    }

    private void SetValue(Chunk chunk ,int x,int y,int index, int variant)
    {
        chunk.grid[x,y].SetTileID(mapGeneratorSettings.tiles[index].tileID,21); 
        chunk.grid[x,y].variant = variant;
    }
    private void SetBuildingObject(Chunk chunk, int x, int y,int index)
    {
        chunk.grid[x, y].SetGridObject(new GridObject(index, 0, null,new Vector2(x,y)));
    }
    private void SetBuildingObject(Chunk chunk, int x, int y, int index, int variant)
    {
        chunk.grid[x, y].SetGridObject(new GridObject(index, variant, null, new Vector2(x, y)));
    }
    private void SetGridHole(Chunk chunk, int x, int y)
    {
        GridTile gridTile = chunk.grid[x, y];
        gridTile.SetSecondLayerID(-1);
        gridTile.SetTileID(60);
        gridTile.SetGridObject(new GridHole(60,null));
    }
    private void GenerateMap(float cellSize, Vector2 offset, out Map map)
    {
        map = new Map(offset, cellSize,chunkSize,widthInChunks,heightInChunks);

        for (int y = 0; y < heightInChunks; y++)
        {
            for (int x = 0; x < widthInChunks; x++)
            {
                int index = x + y * widthInChunks;
                map.chunks.Add(index, new Chunk(index,chunkSize, new Vector2(x * chunkSize,y * chunkSize), offset + new Vector2(x * chunkSize * cellSize, y * chunkSize * cellSize)));
            }
        }

        var rand = new System.Random(seed);
        //List<int> numerVariants = GetNumberVariants();
        List<float> chancesOfDefaultTile = GetChanceOfDefaultTile();

        foreach (var item in map.chunks)
        {
            float value = Generate((int)item.Value.chunkCoordinates.x,(int)item.Value.chunkCoordinates.y, offset, scale);


            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    GenerateCell(item.Value, x, y,rand);
                    if (item.Value.grid[x, y].GridObjectIsType<GridHole>()) continue;

                    int index = -1;
                    if (value >= 0.75f)
                    {
                        index = 0;
                    }
                    else if (value >= 0.5f)
                    {
                        index = 1;
                    }
                    else if (value >= 0.25f)
                    {
                        index = 2;
                    }
                    else 
                    {
                        index = 3;
                    }
                    SetValue(item.Value, x, y, index, rand.Next(6));
                    //    if(numerVariants[index] == 1 || rand.Next(100) / 99f < chancesOfDefaultTile[index])
                    //      SetValue(item.Value, x, y, index, 0);
                    //  else
                    //   SetValue(item.Value, x, y, index,rand.Next(1, numerVariants[index]));
                }
            }

            
        }

        foreach (var item in map.chunks)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    if (item.Value.grid[x, y].GridObjectIsType<GridHole>())
                    {
                        int posX = x + (int)item.Value.chunkCoordinates.x;
                        int posY = y + (int)item.Value.chunkCoordinates.y;
                        GridVisualization.instance.PourWater(1000, new Vector2(posX, posY));
                    }
                }
            }
        }
    }
    private List<int> GetNumberVariants()
    {
        List<int> list = new List<int>();
        for (int i = 0; i < mapGeneratorSettings.tiles.Count; i++)
        {
            list.Add(MapVisualization.instance.TilesUV[mapGeneratorSettings.tiles[i].tileID].variants);
        }
        return list;
    }
    private List<float> GetChanceOfDefaultTile()
    {
        List<float> list = new List<float>();
        for (int i = 0; i < mapGeneratorSettings.tiles.Count; i++)
        {
            list.Add(((Floor)ItemsAsset.instance.GetItem(mapGeneratorSettings.tiles[i].tileID)).chanceOfDefaultTile);
        }
        return list;
    }
    private void GenerateCell(Chunk chunk, int x, int y, System.Random rand)
    {
        //float value = Generate(x + (int)chunk.ChunkGridPosition.x, y + (int)chunk.ChunkGridPosition.y, offset, scale);
        int posX = x + (int)chunk.chunkCoordinates.x;
        int posY = y + (int)chunk.chunkCoordinates.y;
       
        float rainValue = Generate(posX,posY, offsetRain, scaleRain);
        float tempValue = Generate(posX,posY, offsetTemp, scaleTemp);
        float heightValue = Generate(posX,posY, offsetHeight, scaleHeight);

        if(heightValue < 0.15f) SetGridHole(chunk, x, y);
        else if (rand.Next(0, 100) <= 2)
        {
            SetBuildingObject(chunk, x, y, 5);
        }
        else if (rand.Next(0, 100) <= 5)
        {
            SetBuildingObject(chunk, x, y, 70, UnityEngine.Random.Range(0,6));
        }
        else if (rand.Next(0, 100) <= 5)
        {
            SetBuildingObject(chunk, x, y, 71, UnityEngine.Random.Range(0, 6));
        }
        else if (rand.Next(0, 100) <= 2)
        {
            SetBuildingObject(chunk, x, y, 44);
        }
        else if (rand.Next(0, 100) <= 1)
        {
            SetBuildingObject(chunk, x, y, 45);
        }
        else if (rand.Next(0, 100) <= 5)
        {
            SetBuildingObject(chunk, x, y, 46, UnityEngine.Random.Range(0, 6));
        }
        else if (rand.Next(0, 100) <= 1)
        {
            SetBuildingObject(chunk, x, y, 50);
        }
        else if (rand.Next(0, 100) <= 2)
        {
            SetBuildingObject(chunk, x, y, 57);
        }

    }

    //private void GenerateTempCell(Chunk chunk, int x, int y, System.Random rand)
    //{
    //    float value = Generate(x + (int)chunk.ChunkGridPosition.x, y + (int)chunk.ChunkGridPosition.y,offsetRain,scaleRain);
    //    if (value >= 0.8f)
    //    {
    //        SetValue(chunk, x, y, 3);
    //    }
    //    else if (value >= 0.6f)
    //    {
    //        SetValue(chunk, x, y, 4);
    //    }
    //    else if (value >= 0.4f)
    //    {
    //        SetValue(chunk, x, y, 5);
    //    }
    //    else if (value >= 0.2f)
    //    {
    //        SetValue(chunk, x, y, 6);
    //    }
    //    else
    //    {
    //        SetValue(chunk, x, y, 7);
    //    }
    //}

    private float Generate(int x, int y, Vector2 offset, Vector2 scale)
    {
        float xf = ((float)x  + offset.x )/ chunkSize * scale.x;
        float yf = ((float)y + offset.y) / chunkSize * scale.y;
        float value = Mathf.PerlinNoise(xf,yf);
        return value;
    }
    private float Generate(int x, int y, Vector2 offset, float scale)
    {
        float xf = ((float)x + offset.x) / chunkSize * scale;
        float yf = ((float)y + offset.y) / chunkSize * scale;
        float value = Mathf.PerlinNoise(xf, yf);
        return value;
    }
}
