using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct EQMoveItemToContainer : IRpcCommand
{
    public SlotPosition from;
}