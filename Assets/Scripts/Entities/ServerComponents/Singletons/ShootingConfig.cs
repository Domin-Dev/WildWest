
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

[BurstCompile]
public struct ShootingConfig: IComponentData
{
    public float maxSpread;
    public float shootSpread;
    public float changeItemInHandSpread;
    public float sensitivityPlayerMove;
    public float sensitivityPlayerAim;
    public float spreadRecovery;
}