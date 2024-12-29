using UnityEngine;
[CreateAssetMenu(fileName = "Item", menuName = "GameAsset/Items/Item")]
public class Item : ScriptableObject
{
    [Header("Item Stats")]
    public string name;
    [Multiline()]
    public string description;
    public int ID = -1;
    public int stackMax = 50;

    [Header("Item graphic")]
    public Sprite icon;

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

[CreateAssetMenu(fileName = "DestroyableItem", menuName = "GameAsset/Items/DestroyableItem")]
public class Destroyable : Item
{
    [Header("Destroyable")]
    public int durability;
    public override ItemStats GetItemStats()
    {
        return new DestroyableItem(ID,durability);
    }
}

