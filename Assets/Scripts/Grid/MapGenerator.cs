using System;
using Unity.Mathematics;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Seed Settings")]
    [SerializeField] private int seed;
    [SerializeField] private bool randomSeed;
    [Header("Map Size( in chunks )")]
    [SerializeField] private int widthInChunks = 256;
    [SerializeField] private int heightInChunks = 256;
    [Header("Map Generator Settings")]
    [SerializeField] private float scale = 20;
    [SerializeField] private Vector2 offset;
    [Header("Temperature Map Settings")]
    [SerializeField] private float scaleTemp = 20;
    [SerializeField] private Vector2 offsetTemp;
    [Header("Rainfall Map Settings")]
    [SerializeField] private float scaleRain = 20;
    [SerializeField] private Vector2 offsetRain;
    [Header("Chunk Settings")]
    [SerializeField] private int chunkSize = 10;

    private static readonly Vector2 gridOffset = new Vector2(0,0); 

    private MapGeneratorSettings mapGeneratorSettings;
    GridVisualization gridVisualization;
    Grid<GridTile> grid;


    private void Awake()
    {
        if(randomSeed)
        {
            seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }
    }
    private void Start()
    {
        gridVisualization = GridVisualization.instance;
        mapGeneratorSettings = gridVisualization.settings;

        var rand = new System.Random(seed);
        offset.x = rand.Next(-100000, 100000);
        offset.y = rand.Next(-100000, 100000);

        offsetTemp.x = rand.Next(-100000, 100000);
        offsetTemp.y = rand.Next(-100000, 100000);

        offsetRain.x = rand.Next(-100000, 100000);
        offsetRain.y = rand.Next(-100000, 100000);

        var map = GenerateMap(0.25f, gridOffset);
        gridVisualization.SetMap(map); 
    }

    private void SetValue(Chunk chunk ,int x,int y,int index)
    {
        chunk.grid[x,y].tileID = mapGeneratorSettings.tiles[index].tileID;
    }
    
    private void SetBuildingObject(Chunk chunk, int x, int y,int index)
    {
        chunk.grid[x, y].SetGridObject(new GridObject(index, 0, null));
    }

    public Map GenerateMap(float cellSize, Vector2 offset)
    {
        Map map = new Map(offset, cellSize,chunkSize,widthInChunks,heightInChunks);

        for (int y = 0; y < heightInChunks; y++)
        {
            for (int x = 0; x < widthInChunks; x++)
            {
                map.chunks.Add(x + y * widthInChunks,new Chunk(chunkSize,new Vector2(x * chunkSize,y * chunkSize), offset + new Vector2(x * chunkSize * cellSize, y * chunkSize * cellSize)));
            }
        }

        var rand = new System.Random(seed);

        foreach (var item in map.chunks)
        {
            float value = Generate((int)item.Value.ChunkGridPosition.x,(int)item.Value.ChunkGridPosition.y, offset, scale);


            for (int y = 0; y < chunkSize; y++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    GenerateCell(item.Value, x, y,rand);
                    if (value >= 0.75f)
                    { 
                        SetValue(item.Value, x, y,0);
                    }
                    else if (value >= 0.5f)
                    {
                        SetValue(item.Value, x, y, 1);
                    }
                    else if (value >= 0.25f)
                    {
                        SetValue(item.Value, x, y, 2);
                    }
                    else 
                    {
                        SetValue(item.Value, x, y, 3);
                    }
                }
            }
        }
        return map;
    }
    private void GenerateCell(Chunk chunk, int x, int y, System.Random rand)
    {
        //float value = Generate(x + (int)chunk.ChunkGridPosition.x, y + (int)chunk.ChunkGridPosition.y, offset, scale);
        int posX = x + (int)chunk.ChunkGridPosition.x;
        int posY = y + (int)chunk.ChunkGridPosition.y;
        float rainValue = Generate(posX,posY, offsetRain, scaleRain);
        float tempValue = Generate(posX,posY, offsetTemp, scaleTemp);


        //if (rainValue <= 0.3f && tempValue > 0.8f)
        //{
        //    SetValue(chunk, x, y, 8);
        //}
        //else if (rainValue <= 0.3f && tempValue > 0.2f)
        //{
        //    SetValue(chunk, x, y, 1);
        //}
        //else
        //{
        //    SetValue(chunk, x, y, 0);
        //}

        if (rand.Next(0, 100) <= 2)
        {
            SetBuildingObject(chunk, x, y, 5);
        }
        else if (rand.Next(0, 100) <= 2)
        {
            SetBuildingObject(chunk, x, y, 44);
        }
        else if (rand.Next(0, 100) <= 1)
        {
            SetBuildingObject(chunk, x, y, 45);
        }
        else if (rand.Next(0, 100) <= 1)
        {
            SetBuildingObject(chunk, x, y, 46);
        }
        else if (rand.Next(0, 100) <= 1)
        {
            SetBuildingObject(chunk, x, y, 50);
        }

    }

    private void GenerateTempCell(Chunk chunk, int x, int y, System.Random rand)
    {
        float value = Generate(x + (int)chunk.ChunkGridPosition.x, y + (int)chunk.ChunkGridPosition.y,offsetRain,scaleRain);
        if (value >= 0.8f)
        {
            SetValue(chunk, x, y, 3);
        }
        else if (value >= 0.6f)
        {
            SetValue(chunk, x, y, 4);
        }
        else if (value >= 0.4f)
        {
            SetValue(chunk, x, y, 5);
        }
        else if (value >= 0.2f)
        {
            SetValue(chunk, x, y, 6);
        }
        else
        {
            SetValue(chunk, x, y, 7);
        }
    }

    private float Generate(int x, int y, Vector2 offset, float scale)
    {
        float xf = ((float)x  + offset.x )/ chunkSize * scale;
        float yf = ((float)y + offset.y) / chunkSize * scale;
        float value = Mathf.PerlinNoise(xf,yf);
        return value;
    }
}
