using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct EQSelectItem : IRpcCommand
{
    public SlotPosition position;
    public int value;
}