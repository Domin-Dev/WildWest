using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Garment),false)]
public class GarmentEditor : ItemEditor
{
   

    Garment settings;
    public override void OnInspectorGUI()
    {
        settings = (Garment)target;
        if (GUILayout.Button("Cut Sprites"))
        {
            CutSprites(settings.texture);
        }
        base.OnInspectorGUI();
    }
    private void CutSprites(Texture2D texture)
    {
        int id = settings.ID;
        int size = texture.height;

        List<Sprite> spites = new List<Sprite>();
       
        if (!AssetDatabase.IsValidFolder($"{MyTools.clothesSpritesPath}/{id}"))
        {
            AssetDatabase.CreateFolder($"{MyTools.clothesSpritesPath}", id.ToString());
        }
        for (int j = 0; j < 4; j++)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(j * size, 0, size, size), new Vector2(0.5f, 0.5f));
            AssetDatabase.CreateAsset(sprite, $"{MyTools.clothesSpritesPath}/{id}/{settings.type.ToString()}{j}_{id}.asset");
            spites.Add(sprite);
        }
        
        settings.sprites = spites.ToArray();
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(settings);
    }
}
