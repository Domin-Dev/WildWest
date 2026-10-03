using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using NaughtyAttributes;
using Unity.VisualScripting;

[CreateAssetMenu(fileName = "Item", menuName = "GameAsset/Items/Item")]
public class Item : ScriptableObject,ISerializationCallbackReceiver
{
    [Header("Item Stats")]
    public string name;
    [Multiline()]
    public string description;
    public int stackMax = 50;
    public int ID = -1;

    [Header("Item graphic")]
    public Sprite icon;

    public Sprite worldSprite;
    public List<Sprite> animSprites;
    public TagSelection[] tags;

    [Header("Craft recipe")]
    public Ingredient[] crafingIngredients;
    public int[] craftTables;
    public int numberItem = 1;

    public virtual bool HasContainer(out int containerCapacity, out MandatoryProperties mandatoryProperties, out int mandatoryData)
    {
        containerCapacity = 0;
        mandatoryData = 0;
        mandatoryProperties = MandatoryProperties.none;
        return false;
    }
    public virtual Sprite GetWorldSprite 
    {
        get
        {
            if(worldSprite == null)
                return icon;
            else 
                return worldSprite;
        }
    }
    public virtual ItemStats GetItemStats()
    {
        return new ItemStats(ID);
    }
    private void OnValidate()
    {
       if(ID == -1) ID = Resources.Load<IDManager>("IDManager").GetNextID();
    }
    public virtual TooltipInfo GetTooltip(ItemStats itemStats)
    {
        StringBuilder content = new StringBuilder();
        StringBuilder header = new StringBuilder();
        Color? hColor = UIAssetsManager.instance.GetQualityColor(itemStats.quality);

        header.Append(name);
        if (itemStats.quality != Quality.none)
            header.Append($" [ {itemStats.quality.ToString().ToUpper()} ]");

        content.Append(description);

        var tags = ItemsAsset.instance.GetItemTags(itemStats.itemID);
        if (tags.Length > 0)
        {   
            UIStringsHelper.Append(content,UIManager.instance.GetProperty("Tags"), tags);
        }
        // if (itemStats is ItemWithBar)
        // {
        //     ItemWithBar barValue = (ItemWithBar)itemStats;
        //     string bar = ItemsAsset.instance.GetBarName(itemStats.itemID);
        //      UIStringsHelper.Append(content, UIManager.instance.GetColorHexStringForItem(itemStats.itemID), .StringDatabase.GetLocalizedString(Translations.eqTable, bar, fallbackBehavior: FallbackBehavior.UseProjectSettings),bar,barValue.current.ToString("F2") + "/" + barValue.maxValue.ToString("F2"));
        // }
        
        if (itemStats.wetness >= 0.01)
             UIStringsHelper.Append(content, UIManager.instance.GetProperty("Wetness"), itemStats.wetness.ToString("F2") + " %");
         UIStringsHelper.Append(content, UIManager.instance.GetProperty("MaxStack") , stackMax.ToString());
      
        if (itemStats.color.HasValue)
        {
            string colorHex = UnityEngine.ColorUtility.ToHtmlStringRGB(itemStats.color.Value);
            UIStringsHelper.Append(content,UIManager.instance.GetProperty("Color"), UIStringsHelper.GetColorfulString( "#" + colorHex,colorHex));
        }

 
        return new TooltipInfo(content.ToString(), header.ToString(),hColor);
    }
    protected float CalculateTime(List<KeyFrame> keyframes)
    {
        float maxDur = 0;
        foreach (BodyPartType type in Enum.GetValues(typeof(BodyPartType)))
        {
            float timer = 0;
            foreach(var i in keyframes)
            {
                if(i.BodyPartType == type)
                    timer += i.Duration;
            }
            if(timer > maxDur) 
                maxDur = timer;
        }
        return maxDur;
    }
    
    public virtual void SetUp(){}
    public virtual void OnBeforeSerialize()
    {      
    }
    public virtual void OnAfterDeserialize()
    {
    }
}

[System.Serializable]
public class Ingredient : ItemID
{
    public int number;
    public Ingredient(int itemID, int number): base(itemID)
    {
        this.number = number;
    }
}

[System.Serializable]
public class ItemID
{
    public int itemID;

    public ItemID(int itemID)
    {
        this.itemID = itemID;
    }
}



[CreateAssetMenu(fileName = "DestroyableItem", menuName = "GameAsset/Items/DestroyableItem")]
public class Destroyable : Item, IItemBar
{
    [Header("Destroyable")]
    public int durability;
    public int destructionSound;

    public string GetBarName()
    {
        return "Durability";
    }

    public override ItemStats GetItemStats()
    {
        return new ItemWithBar(ID,durability);
    }

    public float GetMaxBarValue()
    {
        return durability;
    }
    public float GetStartBarValue()
    {
        return durability;
    }


}

