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
}


