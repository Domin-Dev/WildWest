using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct EQDropItem : IRpcCommand
{
    public SlotPosition position;
}