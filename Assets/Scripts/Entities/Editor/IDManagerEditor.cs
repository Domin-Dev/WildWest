using Mono.Cecil;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(IDManager), true)]
public class IDManagerEditor : Editor
{
    IDManager iDManager;
    public override void OnInspectorGUI()
    {
        iDManager = (IDManager)target;
        if(GUILayout.Button("Set IDs"))
        {
            Item[] loadedItems = Resources.LoadAll<Item>("Items");
            foreach(Item item in loadedItems)
            {
                item.ID = iDManager.GetNextID();
                item.crafingIngredients = null;
                EditorUtility.SetDirty(item);
             //   item.SetDirty();
            }
            iDManager.SetDirty();
        }
        base.OnInspectorGUI();
        
    }

    
}
