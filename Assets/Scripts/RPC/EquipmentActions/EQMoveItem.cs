using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct EQMoveItem : IRpcCommand
{
    public SlotPosition from;
    public SlotPosition to;
    public int value;
}