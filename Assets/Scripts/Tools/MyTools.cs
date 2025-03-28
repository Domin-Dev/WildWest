using System.Reflection;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Unity.Burst;
using System;

public static class MyTools 
{

    public readonly static Color hitboxColor = new Color(0, 1, 0, 1);
    public readonly static string buildingObjectsSpritesPath = "Assets/Graphics/Sprites/BuildingObjects";
    public readonly static string clothesSpritesPath = "Assets/Graphics/Sprites/Clothes";
    public readonly static string hairstylesSpritesPath = "Assets/Graphics/Sprites/HairStyles";



    public readonly static Vector2[] directions8 = {
        new Vector2(0, 1),
        new Vector2(1, 1),
        new Vector2(1, 0),
        new Vector2(1, -1),
        new Vector2(0, -1),
        new Vector2(-1,-1), 
        new Vector2(-1,0),
        new Vector2(-1,1)
    };
    public readonly static Vector2[] directions4 = {
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(0,-1),
        new Vector2(-1,0),
    };
    public readonly static Vector2[] diagonalDirections = {
        new Vector2(1, 1),
        new Vector2(1, -1),
        new Vector2(-1,-1),
        new Vector2(-1,1)
    };

    [BurstCompile]
    public static Vector3 GetMouseWorldPosition()
    {
        Vector3 pos = Input.mousePosition;
        pos.z = 0;
        pos = Camera.main.ScreenToWorldPoint(pos);
        return pos;
    }
    public static TextMesh CreateText(string text,int fontSize,Vector2 pos,Transform parent,Color color)
    {
        TextMesh textMesh = new GameObject("Text",typeof(TextMesh)).GetComponent<TextMesh>();
        textMesh.transform.position = pos;
        textMesh.transform.parent = parent;
        textMesh.text = text;
        textMesh.characterSize = 0.05f;
        textMesh.fontSize = fontSize;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = color;
        return textMesh;
    }
    public static  void ChangePositionPivot(Transform transform, Vector3 newPosition)
    {
        Transform child = transform.GetChild(0);
        child.SetParent(null);
        transform.position = newPosition;
        child.SetParent(transform);
    }

    public static bool HaveOppositeSigns<T>(T a, T b) where T : struct, IComparable<T>
    {
        double x = Convert.ToDouble(a);
        double y = Convert.ToDouble(b);
        return (x >= 0 && y < 0) || (x < 0 && y >= 0);
    }

}
