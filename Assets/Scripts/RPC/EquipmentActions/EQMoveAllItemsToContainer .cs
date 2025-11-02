using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct EQMoveAllItemsToContainer : IRpcCommand
{
    public int itemID;
    public int containerFrom;
    public int containerTo;
}