

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Linq;
using System.Reflection;

public class TagFinder : ScriptableObject, ISearchWindowProvider
{

    private Action<int> action;

    public TagFinder(Action<int> action)
    {
        this.action = action;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry > result = new List<SearchTreeEntry>();
        result.Add(new SearchTreeGroupEntry(new GUIContent("Tag"), 0));

        var tags = TagList.tags;

        var element = new SearchTreeEntry(new GUIContent("Null [ID: -1]"));
        element.userData = -1;
        element.level = 1;
        element.content.image = null;
        result.Add(element);

        foreach (var item in tags)
        {
            SearchTreeEntry searchTreeEntry = new SearchTreeEntry(new GUIContent($"{item.Value.name} [ID: {item.Key}]"));
            searchTreeEntry.userData = item.Key;
            searchTreeEntry.level = 1;
            searchTreeEntry.content.image = TagList.GetIcon(item.Value);
            result.Add(searchTreeEntry);
        }
    
        return result;
    }
    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        action.Invoke((int)SearchTreeEntry.userData);
        return true;
    }
}
