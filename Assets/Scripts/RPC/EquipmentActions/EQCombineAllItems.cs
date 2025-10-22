using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct EQCombineAllItems : IRpcCommand
{
    public SlotPosition position;
}