using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Linq;
using System.Reflection;

public class ItemFinder : ScriptableObject, ISearchWindowProvider
{

    private Action<int> action;

    public ItemFinder(Action<int> action)
    {
        this.action = action;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry > result = new List<SearchTreeEntry>();
        result.Add(new SearchTreeGroupEntry(new GUIContent("Item"), 0));

        var items = ItemList.items;

        var element = new SearchTreeEntry(new GUIContent("Null [ID: -1]"));
        element.userData = -1;
        element.level = 1;
        element.content.image = null;
        result.Add(element);

        foreach (var item in items)
        {
            SearchTreeEntry searchTreeEntry = new SearchTreeEntry(new GUIContent($"{item.Value.name} [ID: {item.Key}]"));
            searchTreeEntry.userData = item.Key;
            searchTreeEntry.level = 1;
            searchTreeEntry.content.image = GetIcon(item.Value);
            result.Add(searchTreeEntry);
        }
    
        return result;
    }


    private Texture2D GetIcon(Item item)
    {
        if (item.icon == null) return null;
        Rect rect = item.icon.rect;
        var texture = new Texture2D((int)rect.width, (int)rect.height);
        Color[] pixels = item.icon.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        action.Invoke((int)SearchTreeEntry.userData);
        return true;
    }
}
