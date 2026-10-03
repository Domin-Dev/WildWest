using UnityEngine;

[CreateAssetMenu(fileName = "Food", menuName = "GameAsset/Items/Food")]
public class Food : Item, IItemBar
{
    [Header("Food")]
    public int saturation;
    public float shelfLife;

    public string GetBarName()
    {
        return "Freshness";
    }

    public override ItemStats GetItemStats()
    {
        return new ItemWithBar(ID, shelfLife);
    }

    public float GetMaxBarValue()
    {
        return shelfLife;
    }

    public float GetStartBarValue()
    {
        return shelfLife;
    }


}





