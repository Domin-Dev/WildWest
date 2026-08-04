using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

[InitializeOnLoad]

public class TagList : MonoBehaviour
{
    public static Dictionary<int, TagBase> tags
    {
        get
        {
            TagBase[]
            loadedTags = Resources.LoadAll<TagBase>("Tags");
            var tag = new Dictionary<int, TagBase>();
            for (int i = 0; i < loadedTags.Length; i++)
            {
                TagBase item = loadedTags[i];
                if (!tag.ContainsKey(item.ID))
                {
                    tag.Add(item.ID, item);
                }
                else
                {
                    Debug.Log("Error:" + item.ID);
                }
            }
            return tag;

        }
    }

    public static string GetTagName(int ID)
    {
        if(tags.ContainsKey(ID))
        {
            return tags[ID].name;
        }
        else
        {
            return null;
        }
    }


    public static Texture2D GetIcon(TagBase item)
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
        TagBase item;
        if (tags.ContainsKey(itemID))
            item = tags[itemID];     
        else
            return null;
       return GetIcon(item);
    }
}
