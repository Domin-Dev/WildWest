using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Tile
{
    public int tileID;
}

[CreateAssetMenu(fileName = "MapGeneratorSettings", menuName = "GameAsset/Settings/MapGeneratorSettings")]
public class MapGeneratorSettings : ScriptableObject
{
    public int grassID;
    public List<Tile> tiles;


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

    private static readonly Vector2 gridOffset = new Vector2(0, 0);
}


