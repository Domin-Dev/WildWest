using Unity.Mathematics;
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
    [Min(1)]public int bulletCount = 1;
    [Min(0)][SerializeField] private float bulletsSpread = 0;
    [SerializeField]private Sprite bulletSprite;
    public AmmoType type;


    public Sprite BulletSprite => bulletSprite == null ? worldSprite : bulletSprite;
    public float bulletOffset => bulletCount == 1 ? 0 : bulletsSpread / (bulletCount - 1);
    public float BulletsSpread => bulletCount == 1 ? 0 : bulletsSpread;

}
