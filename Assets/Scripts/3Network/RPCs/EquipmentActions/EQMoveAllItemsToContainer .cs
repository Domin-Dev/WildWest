using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct EQMoveAllItemsToContainer : IRpcCommand
{
    public SlotPosition from;
}