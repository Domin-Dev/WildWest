
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



[CustomEditor(typeof(VariantItem),true)]
public class VariantItemEditor : ItemEditor
{
    VariantItem variantItem;
    static readonly Color particlePointColor = new Color(1, 0, 0, 1);


    private void OnEnable()
    {
        variantItem = target as VariantItem;
    }

    Texture2D texture;
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Building Object Texture");

        if (GUILayout.Button("Select texture"))
        {
            string path = EditorUtility.OpenFilePanel("Select texture", "Assets/Resources/Textures", "png,jpg");

            if (!string.IsNullOrEmpty(path))
            {
                string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
                if (texture != null)
                {
                    Debug.Log("The texture is set");
                    CutSpritesWall(texture);
                    NewSaveChanges();
                }
            }
        }

        EditorGUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }

   
    private void CutSpritesWall(Texture2D texture)
    {
        if(texture.width % variantItem.size.x != 0 || texture.height % variantItem.size.y != 0)
        {
            Debug.LogError("Variant size does not match texture size");
        }
        int k = texture.width / variantItem.size.x;
        int h = texture.height / variantItem.size.y;


        List<ObjectVariant> objectVariants = new List<ObjectVariant>();
        AssetDatabase.CreateFolder("Assets/Graphics/Sprites/BuildingObjects/", "NOWYFOL");

        Debug.Log($"{MyTools.buildingObjectsSpritesPath}/{variantItem.name}_{variantItem.ID}");
        if (!AssetDatabase.IsValidFolder($"{MyTools.buildingObjectsSpritesPath}/{variantItem.name}_{variantItem.ID}"))
        {
            AssetDatabase.CreateFolder($"{MyTools.buildingObjectsSpritesPath}", $"{variantItem.name}_{variantItem.ID}");
            AssetDatabase.Refresh();
        }

        Cut(texture,objectVariants,k,(h - 1) / 3);

        variantItem.objectVariants = objectVariants.ToArray();
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(variantItem);
    }


    private void Cut(Texture2D texture, List<ObjectVariant> objectVariants, int k,int numberVariant)
    {
        Color[] pointsColor = { particlePointColor };
        int width = variantItem.size.x;
        int height = variantItem.size.y;


        for (int i = 0; i < k; i++)
        {
            List<Variant> variants = new List<Variant>();
            for (int j = 0; j < numberVariant; j++)
            {
                Sprite[] sprites = new Sprite[3];
                for(int m = 0; m < 3; m ++)
                {
                    sprites[m] = Sprite.Create(texture, new Rect(i * width,texture.height - height * (3 * j + 2 + m), width, height), Vector2.zero);
                }

                Sprite hitbox = Sprite.Create(texture, new Rect(i * width, texture.height - height, width, height), Vector2.zero);
                Cutter cutter = new Cutter(hitbox, Vector2.zero);
                Vector2?[] points = cutter.GetPoints(pointsColor,MyTools.hitboxColor);
                RectangleHitbox rectangle = cutter.CutRectangularHitBox(MyTools.hitboxColor);

                variants.Add(new Variant(rectangle,rectangle != null ? rectangle.GetMinY() : 0, points[0].Value,sprites));
            }

            objectVariants.Add(new ObjectVariant(variants.ToArray()));

            for (int j = 0; j < numberVariant; j++)
            {
                for(int m = 0; m < 3; m ++)
                {
                    AssetDatabase.CreateAsset(variants[j].sprites[m], $"{MyTools.buildingObjectsSpritesPath}/{variantItem.name}_{variantItem.ID}/{variantItem.name}_{i+k*j}_{m}.asset");
                }
            }
        }
    }

    private  void NewSaveChanges()
    { 
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(variantItem); 
    }
}
