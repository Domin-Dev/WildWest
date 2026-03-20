using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;



[CreateAssetMenu(fileName = "VisualEffects", menuName = "GameAsset/ConfigFiles/VisualEffects")]
public class VisualEffects : ScriptableObject
{
    [SerializeField] private List<GameObject> prefabs;
    public List<GameObject> Prefabs => prefabs;
}
