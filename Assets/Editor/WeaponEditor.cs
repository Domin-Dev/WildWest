using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(Weapon),true)]
public class WeaponEditor : ItemEditor
{
    Sprite hitbox;
    int width;
    float pixelSize;
    Vector2 middle;
    Color[] texture;

    static readonly Color gripPoint1Color = new Color(1, 0, 0, 1);
    static readonly Color gripPoint2Color = new Color(0, 0, 1, 1);
    static readonly Color aimPointColor = new Color(1, 1, 0, 1);
    static readonly Color reloadPointColor = new Color(1, 0, 1, 1);

    public override void OnInspectorGUI()
    {
        Weapon weapon = target as Weapon;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Weapon Image");
        weapon.weaponImage = (Sprite)EditorGUILayout.ObjectField(weapon.weaponImage, typeof(Sprite), false, GUILayout.Width(150), GUILayout.Height(150));         
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Weapon HitBox to bake");
        hitbox = (Sprite)EditorGUILayout.ObjectField(hitbox, typeof(Sprite),false);
        EditorGUILayout.EndHorizontal();

        if (hitbox != null) CutHitBox(hitbox,weapon);
        base.OnInspectorGUI();
    }
    private void CutHitBox(Sprite sprite,Weapon weapon)
    {
        hitbox = null;
        weapon.gripPoint2 = new Vector2(-100, -100);

        Color[] colors =
        {
            gripPoint1Color, 
            gripPoint2Color,
            aimPointColor,
            reloadPointColor,
        };

        Cutter cutter = new Cutter(sprite);
        weapon.hitBoxPoints = cutter.CutHitBox(MyTools.hitboxColor);
        Vector2?[] points = cutter.GetPoints(colors, MyTools.hitboxColor);
        if (points[0] != null) weapon.gripPoint1 = (Vector2)points[0];

        if (points[1] != null) weapon.gripPoint2 = (Vector2)points[1];

        if (weapon as RangedWeapon != null)
        {
            RangedWeapon rangedWeapon = weapon as RangedWeapon;
            if (points[2] != null) rangedWeapon.aimPoint = (Vector2)points[2];
            if (points[0] != null) rangedWeapon.reloadPoint = (Vector2)points[3];
        }

        base.SaveChanges();
    }
}
