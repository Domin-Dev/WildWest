using System.Text;
using UnityEngine;


[CreateAssetMenu(fileName = "Garment", menuName = "GameAsset/Items/Garment")]

public class Garment : Destroyable
{
    [Header("Garment Stats")]
    public Texture2D texture;
    public Sprite[] sprites;

    public OutfitStats garmentStats;
    public override TooltipInfo GetTooltip(ItemStats itemStats)
    {
        TooltipInfo tooltipInfo = base.GetTooltip(itemStats);
        StringBuilder content =  new StringBuilder();      

        if(garmentStats.armor > 0) UIStringsHelper.Append(content,UIManager.instance.GetProperty("Armor"),garmentStats.armor.ToString());
        if(garmentStats.aesthetic > 0) UIStringsHelper.Append(content,UIManager.instance.GetProperty("Aesthetic"),garmentStats.aesthetic.ToString());
        if(garmentStats.movementSpeed > 0) UIStringsHelper.Append(content,UIManager.instance.GetProperty("MovementSpeed"),garmentStats.movementSpeed.ToString());
        if(garmentStats.insulation > 0) UIStringsHelper.Append(content,UIManager.instance.GetProperty("Insulation"),garmentStats.insulation.ToString());
        if(garmentStats.waterResistance > 0) UIStringsHelper.Append(content,UIManager.instance.GetProperty("WaterResistance"),garmentStats.waterResistance.ToString());


        tooltipInfo.content += (tooltipInfo.content.Length > 0 && content.Length > 0 ? "\n" : "") + content.ToString();
        return tooltipInfo;
    }
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

