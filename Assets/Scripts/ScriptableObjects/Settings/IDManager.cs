using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "IDManager", menuName = "GameAsset/IDManager")]
public class IDManager : ScriptableObject
{
    public int LastID  = 0;

    public int GetNextID()
    {
        LastID++;
        this.SetDirty();
        return  LastID;
    }
}


