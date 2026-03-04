using GluonGui.WorkspaceWindow.Views.WorkspaceExplorer;
using System.Collections.Generic;
using System.Security.Permissions;
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
        EditorGUILayout.PrefixLabel("Floor Texture");

        Button(ref floor.texturePath,"Select Main Texture");
        Button(ref floor.borderTexturePath,"Select Border Texture");


        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }

    private void Button(ref string texturePath, string ButtonText)
    {
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button(ButtonText))
        {
            string path = EditorUtility.OpenFilePanel(ButtonText, "Assets/Resources/Textures", "png,jpg");

            if (!string.IsNullOrEmpty(path))
            {
                string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
                if (texture != null)
                {
                    texturePath = relativePath.Replace("Assets/Resources/", "").Replace(".png", "");
                    NewSaveChanges();
                }
            }
        }
        EditorGUILayout.EndVertical();
    }

    private  void NewSaveChanges()
    { 
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(floor); 
    }
}
