

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.Rendering;


public struct UV
{
    public Vector2 UV00;
    public Vector2 UV11;
}


public class MapVisualization : MonoBehaviour
{
    [SerializeField] private Material mapMaterial;
    [SerializeField] private Texture2D linesTexture;
    public Dictionary<int, TileUV> tilesUV { get; private set; }
    public Dictionary<int, Vector2> borderUV {  get; private set; }
    public Dictionary<int, int> prioritiesUV {  get; private set; }

    private int bordertextureSize;
    private Vector2 sizeBoxInBordertexture;


    int textureWidth;
    int textureHeight;
    float tileWidth,tileHeight,linetileWidth;
    Texture2D mapTexture;
    public Texture2D borderTexture;
    float width1;
    float height1;

    public static int chunkSize = 10;
    public static int numberOfTiles { 
        get { return chunkSize * chunkSize; }
    }

    public const float cellSize = 0.25f;
    public const float cornerSize = 0.1f;
    public float getCornerDistance { get { return cellSize - 2 * cornerSize; } }
    public Vector2 getCornerVector { get { return new Vector2(cornerSize, cornerSize); } }  


    public static MapVisualization instance { private set; get; }
    public void Awake()
    {
        Application.targetFrameRate = 60;
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
    public Transform CreateMesh( RefRO<FixedChunk> chunk)
    {
        Transform partOfMap = new GameObject("part of map").transform;
        Transform borders = new GameObject("Lines").transform;
        borders.SetParent(partOfMap);

        MeshFilter meshFilter = partOfMap.AddComponent<MeshFilter>();
        MeshFilter bordersMeshFilter = borders.AddComponent<MeshFilter>();

        meshFilter.AddComponent<SortingGroup>().sortingOrder = -10;
        bordersMeshFilter.AddComponent<SortingGroup>().sortingOrder = 0;

        Mesh mesh = new Mesh();
        Mesh bordersMesh = new Mesh();

        meshFilter.transform.position = new Vector3(chunk.ValueRO.worldPosition.x, chunk.ValueRO.worldPosition.y, 10);

        Vector3[] vertices = new Vector3[4 * (numberOfTiles)];
        int[] triangles = new int[6 * (numberOfTiles)];
        Vector2[] uv = new Vector2[4 * (numberOfTiles)];

        Vector3[] borderVertices = new Vector3[48 * (numberOfTiles)];
        int[] borderTriangles = new int[72 * (numberOfTiles)];
        Vector2[] borderUV = new Vector2[48 * (numberOfTiles)];

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int index = x + y * chunkSize;
                int tileID = chunk.ValueRO[x, y].tileID;

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

                UV[] uvs = GetBorderUVs(GetNeighbors(x, y, chunk), tileID);
                SetTileBorders(uvs,borderUV, borderTriangles, borderVertices,index, new Vector2(x * cellSize, y * cellSize));
               
                //   int borders = CalculateBorders(x, y, gridTile.tileID);
                Vector2 uv11, uv00;
                GetUVTile(tileID, chunk.ValueRO[x,y].variant, out uv00, out uv11);
                UVSet(uv, index, uv00, uv11);
            }
        }


        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        bordersMesh.vertices = borderVertices;
        bordersMesh.uv = borderUV;
        bordersMesh.triangles = borderTriangles;

        MeshRenderer meshRenderer = meshFilter.AddComponent<MeshRenderer>();
        meshRenderer.material = mapMaterial;
        meshRenderer.material.mainTexture = mapTexture;

        MeshRenderer bordersMeshRenderer = bordersMeshFilter.AddComponent<MeshRenderer>();
        bordersMeshRenderer.material = mapMaterial;
        bordersMeshRenderer.material.mainTexture = borderTexture;

        bordersMeshFilter.mesh = bordersMesh;

        meshFilter.transform.parent = transform;
        meshFilter.mesh = mesh;
        return meshFilter.transform;
    }


    private UV[] GetBorderUVs(int[] neighbors, int tileID)
    {
        UV[] uvs = new UV[12];
        UV nullUV = new UV() { UV00 = new Vector2(1 - sizeBoxInBordertexture.x * 0.1f , 0), UV11 = new Vector2(1, 0) }; 


        for (int i = 0; i < neighbors.Length; i++)
        {
            int neighbor = neighbors[i];
            UV newUV = new UV();
            if (i % 2 == 0)
            {
                if (neighbor == tileID || neighbor == -1)
                {
                    newUV = nullUV;
                }
                else
                {
                    Vector2 borderUv = borderUV[neighbor];
                    switch (i / 2)
                    { 
                        case 0:
                            newUV.UV00 = borderUv + new Vector2(sizeBoxInBordertexture.x * 4, sizeBoxInBordertexture.y * 3);
                            newUV.UV11 = borderUv + new Vector2(sizeBoxInBordertexture.x * 4.5f , sizeBoxInBordertexture.y * 4);
                            break;
                        case 1:
                            newUV.UV00 = borderUv + new Vector2(sizeBoxInBordertexture.x * 4, sizeBoxInBordertexture.y * 2.5f);
                            newUV.UV11 = borderUv + new Vector2(sizeBoxInBordertexture.x * 5f, sizeBoxInBordertexture.y * 3f);
                            break;
                        case 2:
                            newUV.UV00 = borderUv + new Vector2(sizeBoxInBordertexture.x * 4.5f, sizeBoxInBordertexture.y * 3);
                            newUV.UV11 = borderUv + new Vector2(sizeBoxInBordertexture.x * 5f, sizeBoxInBordertexture.y * 4);
                            break;
                        case 3:
                            newUV.UV00 = borderUv + new Vector2(sizeBoxInBordertexture.x * 4, sizeBoxInBordertexture.y * 2f);
                            newUV.UV11 = borderUv + new Vector2(sizeBoxInBordertexture.x * 5f, sizeBoxInBordertexture.y * 2.5f);
                            break;
                    }

                }
                uvs[i] = newUV;
            }
            else
            {
                int k = Mathf.CeilToInt(i / 2f);
                int[] nextAndPrevious = GetNextAndPrevious(neighbors, i);

                if (!(nextAndPrevious[0] == nextAndPrevious[1] && nextAndPrevious[1] == nextAndPrevious[2]))
                {
                    float y = sizeBoxInBordertexture.y * (k - 1);
                    float maxY = sizeBoxInBordertexture.y * (k);


                    if (nextAndPrevious[0] == nextAndPrevious[2] && nextAndPrevious[0] != -1)
                    {
                        Vector2 borderUv = borderUV[nextAndPrevious[0]];
                        newUV.UV00 = borderUv + new Vector2(sizeBoxInBordertexture.x * 2 ,y);
                        newUV.UV11 = borderUv + new Vector2(sizeBoxInBordertexture.x * 3, maxY);
                    }
                    else if(nextAndPrevious[0] != -1)
                    {
                        Vector2 borderUv = borderUV[nextAndPrevious[0]];
                        newUV.UV00 = borderUv + new Vector2(0, y);
                        newUV.UV11 = borderUv + new Vector2(sizeBoxInBordertexture.x, maxY);
                    }
                    else if (nextAndPrevious[2] != -1)
                    {
                        Vector2 borderUv = borderUV[nextAndPrevious[2]];
                        newUV.UV00 = borderUv + new Vector2(sizeBoxInBordertexture.x , y);
                        newUV.UV11 = borderUv + new Vector2(sizeBoxInBordertexture.x * 2, maxY);
                    }
                }
                else
                    uvs[k] = nullUV;
            }
        }
        return uvs;
    }

    private int[] GetNextAndPrevious(int[] neighbors, int index)
    {
        int[] values = new int[3];
        values[1] = neighbors[index];

        if (index - 1 >= 0)
            values[0] = neighbors[index - 1];
        else
            values[0] = neighbors[neighbors.Length - 1];

        if (index + 1 < neighbors.Length)
            values[2] = neighbors[index + 1];
        else
            values[2] = neighbors[0];
        return values;
    }
    private int[] GetNeighbors(int x,int y, RefRO<FixedChunk> chunk)
    {
        int[] neighbors = new int[8];
        Vector2 position = new Vector2(x,y); 
        for (int i = 0; i < 8; i++)
        {
            Vector2 v = MyTools.directions8[i] + position;
            if(v.x < 10 && v.x >= 0 && v.y < 10 && v.y >= 0)
            {
                int id = chunk.ValueRO[(int)v.x,(int)v.y].tileID;
                if (borderUV.ContainsKey(id))
                {
                    neighbors[i] = id;
                    continue;
                }
            }
                neighbors[i] = -1;
        }
        return neighbors;
    }
    ///
    private void SetTileBorders(UV[] UVset,Vector2[] uv,int[] borderTriangles, Vector3[] vertices,int index, Vector2 startPos)
    {
        int startIndex = index * 12;
        SetVertices(UVset[1], uv, borderTriangles,vertices, startPos,getCornerVector, ref startIndex);
        SetVertices(UVset[1],uv, borderTriangles,vertices, startPos,getCornerVector, ref startIndex,1);

        SetVertices(UVset[4],uv, borderTriangles,vertices, startPos + new Vector2(cornerSize,0), new Vector2(getCornerDistance, cornerSize),ref startIndex);
        
        SetVertices(UVset[3],uv, borderTriangles, vertices, startPos + new Vector2(cornerSize + getCornerDistance, 0), getCornerVector, ref startIndex);
        SetVertices(UVset[1],uv, borderTriangles, vertices, startPos + new Vector2(cornerSize + getCornerDistance, 0), getCornerVector, ref startIndex,1);

        startPos += new Vector2(0, cornerSize);

        SetVertices(UVset[6],uv, borderTriangles, vertices, startPos, new Vector2(cornerSize, getCornerDistance), ref startIndex);
        SetVertices(UVset[2],uv, borderTriangles, vertices, startPos + new Vector2(cornerSize + getCornerDistance, 0), new Vector2(cornerSize, getCornerDistance), ref startIndex);

        startPos += new Vector2(0, getCornerDistance);

        SetVertices(UVset[5], uv, borderTriangles, vertices, startPos, getCornerVector, ref startIndex);
        SetVertices(UVset[1], uv, borderTriangles, vertices, startPos, getCornerVector, ref startIndex,1);

        SetVertices(UVset[0],uv, borderTriangles, vertices, startPos + new Vector2(cornerSize, 0), new Vector2(getCornerDistance, cornerSize), ref startIndex);

        SetVertices(UVset[7], uv, borderTriangles, vertices, startPos + new Vector2(cornerSize + getCornerDistance, 0), getCornerVector, ref startIndex);
        SetVertices(UVset[1], uv, borderTriangles, vertices, startPos + new Vector2(cornerSize + getCornerDistance, 0), getCornerVector, ref startIndex,1);
    }
    private void SetVertices(UV set ,Vector2[] uv ,int[] triangles,Vector3[] vertices, Vector2 startPos,Vector2 size, ref int startIndex,int z = 0)
    {
        vertices[startIndex * 4 + 0] = new Vector3(startPos.x, startPos.y,z);
        vertices[startIndex * 4 + 1] = new Vector3(startPos.x, startPos.y + size.y,z);
        vertices[startIndex * 4 + 2] = new Vector3(startPos.x + size.x, startPos.y + size.y,z);
        vertices[startIndex * 4 + 3] = new Vector3(startPos.x + size.x, startPos.y,z);

        triangles[startIndex * 6] = startIndex * 4;
        triangles[startIndex * 6 + 1] = startIndex * 4 + 1;
        triangles[startIndex * 6 + 2] = startIndex * 4 + 2;

        triangles[startIndex * 6 + 3] = startIndex * 4;
        triangles[startIndex * 6 + 4] = startIndex * 4 + 2;
        triangles[startIndex * 6 + 5] = startIndex * 4 + 3;

        uv[startIndex * 4] = set.UV00;
        uv[startIndex * 4 + 1] = new Vector2(set.UV00.x, set.UV11.y);
        uv[startIndex * 4 + 2] = set.UV11;
        uv[startIndex * 4 + 3] = new Vector2(set.UV11.x, set.UV00.y);

        startIndex += 1;    
    }
    private void SetUpMapMaterial(int sizeTile = 25)
    {
        tilesUV = new Dictionary<int, TileUV>();
        Floor[] array = ItemsAsset.instance.GetItemsByType<Floor>();
        Dictionary<int, Texture2D> textures = TextureLoader.LoadFloors(array);
        LoadBorderTextures(array);


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
        tilesUV.Add(-1, new TileUV(uv00, variants, null));
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

            if (floor.ID >= 0) tilesUV.Add(floor.ID, new TileUV(uv00, variants, UVsecond));
        }
        texture.Apply(true, true);
        mapTexture = texture;

        TextureLoader.UnloadTextures(textures);
    }
    private void LoadBorderTextures(Floor[] floors)
    {
        prioritiesUV = new Dictionary<int, int>();
        Dictionary<int, Texture2D> textures = TextureLoader.LoadBorderTextures(floors,prioritiesUV);
        borderUV = new Dictionary<int, Vector2>();


        bordertextureSize = (int)Mathf.CeilToInt(math.sqrt(textures.Count));
        Texture2D t = textures.First().Value;

        int maxWidth = t.width * bordertextureSize;
        int maxHeight = t.height * bordertextureSize;
        sizeBoxInBordertexture = new Vector2(t.width * 0.2f/ maxWidth, t.height * 0.25f / maxHeight); 



        Texture2D texture = new Texture2D(maxWidth, maxHeight);
        texture.filterMode = FilterMode.Point;
        Vector2 uv00 = new Vector2(0, 0);

        int i = 0;
        foreach (var tex in textures)
        {
            int w = (i % bordertextureSize) * t.width;
            int h = (i / bordertextureSize) * t.height;
            uv00 = new Vector2(w / maxWidth, h / maxHeight);
            CopyTexture(w,h, tex.Value, texture);
            borderUV.Add(tex.Key, uv00);
        }
        texture.Apply(true, true);
        borderTexture = texture;

        Debug.LogError(texture + " " + texture.width + " " + texture.height + " " + textures.Count);
        TextureLoader.UnloadTextures(textures);
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
    private void CopyTexture(int x, int y, Texture2D from, Texture2D to)
    {
        Color32[] grassColors = from.GetPixels32();
        to.SetPixels32(x, y, from.width, from.height, grassColors);
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
        uv = tilesUV[tileID].uv00;
        uv00 = uv + (new Vector2(tileWidth, 0) * variant);
        uv11 = (uv + new Vector2(tileWidth, tileHeight)) + (new Vector2(tileWidth, 0) * variant);
    }
}

