using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal.Profiling.Memory.Experimental;

[InitializeOnLoad]

public class ItemList : MonoBehaviour
{
    public static Dictionary<int,Item> items { private set; get; }

    public static string GetItemName(int ID)
    {
        if(items.ContainsKey(ID))
        {
            return items[ID].name;
        }
        else
        {
            return null;
        }
    }

    static ItemList()
    {
        Item[] loadedItems = Resources.LoadAll<Item>("Items");
        items = new Dictionary<int,Item>();
        for (int i = 0; i < loadedItems.Length; i++)
        {
            Item item = loadedItems[i];
            if (!items.ContainsKey(item.ID))
            {
                items.Add(item.ID, item);
            }
            else
            {
                Debug.Log("Error:" + item.ID);
            }
        }
    }

    public static Texture2D GetIcon(Item item)
    {
        if (item.icon == null) return null;
        Rect rect = item.icon.rect;
        var texture = new Texture2D((int)rect.width, (int)rect.height);
        Color[] pixels = item.icon.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    public static Texture2D GetIcon(int itemID)
    {
        Item item;
        if (items.ContainsKey(itemID))
            item = items[itemID];     
        else
            return null;

       return GetIcon(item);
    }
}
