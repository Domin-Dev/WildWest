using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct EQDoubleClickAction : IRpcCommand
{
    public SlotPosition position;
    public bool action;
}