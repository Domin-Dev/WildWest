using System.Reflection;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Unity.Burst;
using System;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Newtonsoft.Json.Linq;

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
    public static float3 QuaternionToEuler(quaternion q)
    {
        float sinr_cosp = 2 * (q.value.w * q.value.x + q.value.y * q.value.z);
        float cosr_cosp = 1 - 2 * (q.value.x * q.value.x + q.value.y * q.value.y);
        float x = math.atan2(sinr_cosp, cosr_cosp);

        float sinp = 2 * (q.value.w * q.value.y - q.value.z * q.value.x);
        float y = math.abs(sinp) >= 1 ? math.sign(sinp) * math.PI / 2 : math.asin(sinp);

        float siny_cosp = 2 * (q.value.w * q.value.z + q.value.x * q.value.y);
        float cosy_cosp = 1 - 2 * (q.value.y * q.value.y + q.value.z * q.value.z);
        float z = math.atan2(siny_cosp, cosy_cosp);

        return new float3(x, y, z);
    }

    public static bool EqualQuaternions(quaternion q1, quaternion q2, float toleranceThreshold = 0.999f)
    {
        float dot = math.dot(q1.value, q2.value);
        return math.abs(dot) > toleranceThreshold;
    }
    public static bool EqualFloat3(float3 a, float3 b, float tolerance = 0.001f)
    {
        return math.all(math.abs(a - b) < new float3(tolerance));
    }

}
