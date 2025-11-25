using UnityEngine;


[CreateAssetMenu(fileName = "Garment", menuName = "GameAsset/Items/Garment")]

public class Garment : Destroyable
{
    [Header("Garment Stats")]
    public Texture2D texture;
    public Sprite[] sprites;

    public int armor = 0;
    public int 	movementSpeed = 0;
    public int insulation = 0; 
    public int waterResistance = 0;


}

public enum GarmentType
{
    headwear,
    faceCover,
    outerwear,
    shirt,
    pants,
    belt,
    accessory,
    bag
}

