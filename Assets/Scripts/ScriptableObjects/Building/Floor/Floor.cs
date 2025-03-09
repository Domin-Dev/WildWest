using UnityEngine;

[CreateAssetMenu(fileName = "Floor", menuName = "GameAsset/Items/BuildingItems/Surface/Floor")]
public class Floor : BuildingItem 
{
    [Range(0.0f, 1.0f)]
    public float chanceOfDefaultTile;
    public GameObject diggingParticles;
    public bool canBeCultivated;
    public string texturePath;
} 

