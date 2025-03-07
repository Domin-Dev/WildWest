using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;



[CustomEditor(typeof(Floor),true)]
public class FloorEditor : ItemEditor
{
    Floor floor;    
    private void OnEnable()
    {
        floor = target as Floor;
    }

    Texture2D texture;
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Floor Texture");

        if (GUILayout.Button("Select texture"))
        {
            string path = EditorUtility.OpenFilePanel("Select texture", "Assets/Resources/Textures", "png,jpg");

            if (!string.IsNullOrEmpty(path))
            {
                string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
                if (texture != null)
                {
                    floor.texturePath = relativePath.Replace("Assets/Resources/", "").Replace(".png","");
                    Debug.Log("The texture is set");
                    NewSaveChanges();
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }

    private  void NewSaveChanges()
    { 
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(floor); 
    }
}
