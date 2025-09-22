using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


[CreateAssetMenu(fileName = "NewTag", menuName = "GameAsset/Tag")]
public class Tag : ScriptableObject
{
    [Header("Tag info")]
    public Sprite icon;
    public string name;
    public int ID = -1;
    private void OnValidate()
    {
        if (ID == -1) ID = Resources.Load<IDManager>("IDManager").GetNextTagID();
    }
}