

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;


public class MapVisualization : MonoBehaviour
{
    [SerializeField] private Material mapMaterial;
    [SerializeField] private Texture2D linesTexture;
    public Dictionary<int, TileUV> TilesUV { get; private set; }



    int textureWidth;
    int textureHeight;
    float tileWidth,tileHeight,linetileWidth;
    Texture2D mapTexture;
    float width1;
    float height1;



    public static int chunkSize = 10;
    public static int numberOfTiles { 
        get { return chunkSize * chunkSize; }
    }

    public const float cellSize = 0.25f;

    public static MapVisualization instance { private set; get; }


    public void Awake()
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
        SetUpMapMaterial();
    }

    public Transform CreateMesh(FixedChunk chunk)
    {
        Transform partOfMap = new GameObject("part of map").transform;
        Transform lines = new GameObject("Lines").transform;
        lines.SetParent(partOfMap);

        MeshFilter meshFilter = partOfMap.AddComponent<MeshFilter>();
        MeshFilter linesMeshFilter = lines.AddComponent<MeshFilter>();

        meshFilter.AddComponent<SortingGroup>().sortingOrder = -10;
        linesMeshFilter.AddComponent<SortingGroup>().sortingOrder = 0;

        Mesh mesh = new Mesh();
        Mesh linesMesh = new Mesh();

        meshFilter.transform.position = new Vector3(chunk.worldPosition.x, chunk.worldPosition.y, 10);

        Vector3[] vertices = new Vector3[4 * (numberOfTiles)];
        int[] triangles = new int[6 * (numberOfTiles)];

        Vector2[] uv = new Vector2[4 * (numberOfTiles)];
        Vector2[] linesUV = new Vector2[4 * (numberOfTiles)];

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int index = x + y * chunkSize;
                vertices[index * 4 + 0] = new Vector3(x * cellSize, y * cellSize);
                vertices[index * 4 + 1] = new Vector3(x * cellSize, (y + 1) * cellSize);
                vertices[index * 4 + 2] = new Vector3((x + 1) * cellSize, (y + 1) * cellSize);
                vertices[index * 4 + 3] = new Vector3((x + 1) * cellSize, y * cellSize);

                triangles[index * 6] = index * 4;
                triangles[index * 6 + 1] = index * 4 + 1;
                triangles[index * 6 + 2] = index * 4 + 2;

                triangles[index * 6 + 3] = index * 4;
                triangles[index * 6 + 4] = index * 4 + 2;
                triangles[index * 6 + 5] = index * 4 + 3;

                //   int borders = CalculateBorders(x, y, gridTile.tileID);

                Vector2 uv11, uv00;


                //if (gridTile.GridObjectIsType<GridHole>(out GridHole hole))
                //{
                //    GridTile tile = GetTileByGridPosition(gridTile.x, gridTile.y + 1);
                //    if (tile != null && tile.GridObjectIsType<GridHole>())
                //    {
                //        GetUVTile(gridTile, 1 + hole.waterLevel * 2, out uv00, out uv11);
                //    }
                //    else
                //    {
                //        GetUVTile(gridTile, 0 + hole.waterLevel * 2, out uv00, out uv11);
                //    }
                //}
                //else
              GetUVTile(chunk[x, y].tileID, chunk[x,y].variant, out uv00, out uv11);


                 //UVSet(uv, index, uv00, uv11);

                //   borders = CalculateBorders(x + (int)chunk.chunkCoordinates.x, y + (int)chunk.chunkCoordinates.y, gridTile.tileID);
                // GetUVLine(borders, out uv00, out uv11);
               UVSet(uv, index, uv00, uv11);
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


    private void SetUpMapMaterial(int sizeTile = 25)
    {
        TilesUV = new Dictionary<int, TileUV>();
        Floor[] array = ItemsAsset.instance.GetItemsByType<Floor>();
        Dictionary<int, Texture2D> textures = TextureLoader.LoadFloors(array);

        Texture2D texture = new Texture2D(MaxWidth(textures), sizeTile * (textures.Count + 1));
        texture.filterMode = FilterMode.Point;

        textureWidth = texture.width;
        textureHeight = texture.height;
        tileWidth = (float)sizeTile / textureWidth;
        tileHeight = (float)sizeTile / textureHeight;
        width1 = 0.01f / textureWidth;
        height1 = 0.01f / textureHeight;

        Vector2 uv00 = new Vector2(0, 0);
        int variants = linesTexture.width / sizeTile;
        TilesUV.Add(-1, new TileUV(uv00, variants, null));
        linetileWidth = (float)sizeTile / linesTexture.width;

        int k = 0;
        for (int i = 0; i < array.Length; i++)
        {
            Floor floor = array[i];
            Texture2D floorTexture = textures[floor.ID];

            Vector2? UVsecond = null;
            if ((floor as Farmland)?.wateredFarmland != null)
            {
                UVsecond = new Vector2(0, (float)k * sizeTile / textureHeight);
                CopyTexture(ref k, sizeTile, (floor as Farmland).wateredFarmland, texture);
            }
            uv00 = new Vector2(0, (float)k * sizeTile / textureHeight);
            CopyTexture(ref k, sizeTile, floorTexture, texture);
            variants = floorTexture.width / sizeTile;

            if (floor.ID >= 0) TilesUV.Add(floor.ID, new TileUV(uv00, variants, UVsecond));
        }
        texture.Apply(true, true);
        mapTexture = texture;

        TextureLoader.UnloadFloors(textures);
    }
    private int MaxWidth(Dictionary<int, Texture2D> array)
    {
        int max = linesTexture.width;
        foreach (var floor in array)
        {
            if (max < floor.Value.width) max = floor.Value.width;
        }
        return max;
    }
    private void CopyTexture(ref int index, int height, Texture2D from, Texture2D to)
    {
        int width = from.width;
        Color32[] grassColors = from.GetPixels32();
        to.SetPixels32(0, index * height, width, height, grassColors);
        index++;
    }
    private void UVSet(Vector2[] uv, int index, Vector2 uv00, Vector2 uv11)
    {
        uv[index * 4] = new Vector2(uv00.x + width1, uv00.y + height1);
        uv[index * 4 + 1] = new Vector2(uv00.x + width1, uv11.y);
        uv[index * 4 + 2] = new Vector2(uv11.x, uv11.y);
        uv[index * 4 + 3] = new Vector2(uv11.x, uv00.y + height1);
    }
    private void GetUVTile(int tileID , int variant, out Vector2 uv00, out Vector2 uv11)
    {
        Vector2 uv;
        uv = TilesUV[tileID].uv00;
        uv00 = uv + (new Vector2(tileWidth, 0) * variant);
        uv11 = (uv + new Vector2(tileWidth, tileHeight)) + (new Vector2(tileWidth, 0) * variant);
    }
}

