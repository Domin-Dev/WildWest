using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item), true)]
public class ItemEditor : Editor
{

    private SerializedProperty tags;
    public void OnEnable()
    {
        tags = serializedObject.FindProperty("tags");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        IconField(target);
        DrawDefaultInspector();
        

       //SerializedProperty property = serializedObject.GetIterator();
        // bool enterChildren = true;
        // while (property.NextVisible(enterChildren))
        // {
        //     enterChildren = false;
      
        //     if (property.name == "tags")
        //         DrawTags();
        //     else
        //         EditorGUILayout.PropertyField(property, true);
        // }

        serializedObject.ApplyModifiedProperties();
    }


    // private void DrawTags()
    // {
    //     tags.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(
    //         tags.isExpanded,
    //         $"Tags ({tags.arraySize})"
    //     );

    //     if (tags.isExpanded)
    //     {
    //         EditorGUI.indentLevel++;
    //         for (int i = 0; i < tags.arraySize; i++)
    //         {
    //             SerializedProperty element =
    //                 tags.GetArrayElementAtIndex(i);

    //             string label = "Empty";
    //             if (element.managedReferenceValue != null)
    //                 label = element.managedReferenceValue.GetType().Name;
                
    //             EditorGUILayout.PropertyField(element, new GUIContent(label),true);
    //         }

    //         if (GUILayout.Button("Add Tag"))
    //             ShowTagMenu();
    //         EditorGUI.indentLevel--;
    //     }
    //     EditorGUILayout.EndFoldoutHeaderGroup();

    // }

    // private void ShowTagMenu()
    // {
    //     GenericMenu menu = new GenericMenu();
    //     var types = TypeCache.GetTypesDerivedFrom<TagSelection>() .Where(t => !t.IsAbstract).ToList();
    //     types.Insert(0, typeof(TagSelection));

    //     foreach (System.Type type in types)
    //     {
    //         menu.AddItem(
    //             new GUIContent(type.Name),
    //             false,
    //             () =>
    //             {
    //                 serializedObject.Update();
    //                 tags.arraySize++;
    //                 SerializedProperty element = tags.GetArrayElementAtIndex(tags.arraySize - 1);
    //                 element.managedReferenceValue = System.Activator.CreateInstance(type);
    //                 serializedObject.ApplyModifiedProperties();
    //             }
    //         );
    //     }
    //     menu.ShowAsContext();
    // }


    public static void IconField(Object target)
    {
        Item item = (Item)target;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Icon Image");
        EditorGUI.BeginChangeCheck();
        Sprite newIcon = (Sprite)EditorGUILayout.ObjectField(item.icon, typeof(Sprite), false, GUILayout.Width(150), GUILayout.Height(150));
        if (EditorGUI.EndChangeCheck()) 
        {
            item.icon = newIcon;
            item.SetDirty(); 
        }
        EditorGUILayout.EndHorizontal();
    }
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        Item item = target as Item;
        if (item.icon == null) return null;


        var texture = new Texture2D(width, height);
        Rect rect = item.icon.rect;
        Color[] pixels = item.icon.texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        if (pixels.Length != width * height)
        {
            pixels = ResizePixels(pixels, (int)rect.width, (int)rect.height, width, height);
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    private Color[] ResizePixels(Color[] originalPixels, int originalWidth, int originalHeight, int newWidth, int newHeight)
    {
        Color[] newPixels = new Color[newWidth * newHeight];
        float scaleX = (float)originalWidth / newWidth;
        float scaleY = (float)originalHeight / newHeight;

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                int originalX = Mathf.FloorToInt(x * scaleX);
                int originalY = Mathf.FloorToInt(y * scaleY);
                newPixels[y * newWidth + x] = originalPixels[originalY * originalWidth + originalX];
            }
        }
        return newPixels;
    }




}
