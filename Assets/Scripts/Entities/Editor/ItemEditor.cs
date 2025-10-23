using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item), true)]
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        IconField(target);
        
        base.OnInspectorGUI();
    }

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
