using UnityEngine;

public enum AmmoType
{
    Revolver,
    Shotgun,
    Rifle,
}

[CreateAssetMenu(fileName = "Ammo", menuName = "GameAsset/Items/Weapons/Ammo")]
public class Ammo : Item
{
    public GameObject bullet;  
    public Sprite UIBulletIcon;
    public int damage;
    public AmmoType type;

}
