using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Item", menuName = "GameAsset/Items/Item")]
public class Item : ScriptableObject
{
    [Header("Item Stats")]
    public string name;
    [Multiline()]
    public string description;
    public int stackMax = 50;
    public int ID = -1;

    [Header("Item graphic")]
    public Sprite icon;

    [Header("Item Tags")]
    public List<TagSelection> tags;

    [Header("Craft recipe")]
    public Ingredient[] crafingIngredients;
    public int[] craftTables;
    public int numberItem = 1;
    public virtual ItemStats GetItemStats()
    {
        return new ItemStats(ID);
    }

    private void OnValidate()
    {
       if(ID == -1) ID = Resources.Load<IDManager>("IDManager").GetNextID();
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

[System.Serializable]
public class TagSelection
{
    public int tagID;

    public TagSelection(int tagID)
    {
        this.tagID = tagID;
    }
}

[CreateAssetMenu(fileName = "DestroyableItem", menuName = "GameAsset/Items/DestroyableItem")]
public class Destroyable : Item, IItemBar
{
    [Header("Destroyable")]
    public int durability;
   
    public override ItemStats GetItemStats()
    {
        return new DestroyableItem(ID,durability);
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

