using UnityEngine;

[CreateAssetMenu(fileName = "Food", menuName = "GameAsset/Items/Food")]
public class Food : Item
{
    [Header("Food")]
    public int saturation;
    public int shelfLife;

    public override ItemStats GetItemStats()
    {
        return new FoodItem(ID, shelfLife);
    }
}





