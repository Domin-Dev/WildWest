using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;


public struct EQOnEquipRPC : IRpcCommand
{
    public SlotPosition position;
}