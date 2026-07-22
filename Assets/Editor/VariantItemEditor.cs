
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



[CustomEditor(typeof(VariantItem),true)]
public class VariantItemEditor : ItemEditor
{
    VariantItem variantItem;
    static readonly Color particlePointColor = new Color(1, 0, 0, 1);
    static readonly Color shadowPointColor = new Color(0, 0, 1, 1);


    private void OnEnable()
    {
        variantItem = target as VariantItem;
    }

    Texture2D texture;
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Building Object Texture");

        if (GUILayout.Button("Cut texture"))
        {
            // string path = EditorUtility.OpenFilePanel("Select texture", "Assets/Resources/Textures", "png,jpg");

            // if (!string.IsNullOrEmpty(path))
            // {
            //     string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            //     texture = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
            // //     if (texture != null)
            // //     {
            //         Debug.Log("The texture is set");
            //         CutSpritesWall(texture);
            //         NewSaveChanges();
            //     }
            // }
            if(variantItem.texture != null)
            {
                texture = variantItem.texture;
                CutSpritesWall(texture);
                NewSaveChanges();
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

        Cut(texture,objectVariants,k, (h - 1) / variantItem.damageStates);

        variantItem.objectVariants = objectVariants.ToArray();
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(variantItem);
    }


    private void Cut(Texture2D texture, List<ObjectVariant> objectVariants, int k,int numberVariant)
    {
        Color[] pointsColor = {shadowPointColor};
        int width = variantItem.size.x;
        int height = variantItem.size.y;


        for (int i = 0; i < k; i++)
        {
            List<Variant> variants = new List<Variant>();
            for (int j = 0; j < numberVariant; j++)
            {

                Sprite[] sprites = new Sprite[variantItem.damageStates];
                int min = 0;
                Vector2 pivot = Vector2.zero;
                for(int m = 0; m < sprites.Length; m ++)
                {
                    var rect = new Rect(i * width,texture.height - height * (sprites.Length * j + 2 + m), width, height);
                    var sprite = Sprite.Create(texture,rect,Vector2.zero);
                    min = new Cutter(sprite).GetMin();
                    pivot = new Vector2(0.5f,(float)min/height);
                    sprites[m] = Sprite.Create(texture,rect,pivot);
                }

                Sprite hitbox = Sprite.Create(texture, new Rect(i * width, texture.height - height, width, height),pivot);
                Cutter cutter = new Cutter(hitbox,new Vector2(width/2,min));
                Vector2?[] points = cutter.GetPoints(pointsColor,MyTools.hitboxColor);
                Vector2[] particlePoints = cutter.GetPoints(particlePointColor,MyTools.hitboxColor);

                RectangleHitbox rectangle = cutter.CutRectangularHitBox(MyTools.hitboxColor);


                variants.Add(new Variant(variantItem.shadowSize,rectangle,min * 0.01f,particlePoints,points[0],sprites));
            }

            objectVariants.Add(new ObjectVariant(variants.ToArray()));

            for (int j = 0; j < numberVariant; j++)
            {
                for(int m = 0; m < variantItem.damageStates; m ++)
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
