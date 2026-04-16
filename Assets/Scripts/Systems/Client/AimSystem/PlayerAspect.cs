using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine.Experimental.GlobalIllumination;




public readonly partial struct PlayerAspect : IAspect
{

    public readonly RefRO<PlayerInputSync> playerInputSync;

    public readonly RefRO<LocalTransform> playerPosition;
    public readonly RefRO<LocalToWorld> localToWorld;
    public readonly RefRW<Hands> hands;
    public readonly RefRW<Character> character;
    public readonly RefRW<Player> player;
    private readonly DynamicBuffer<InputBufferData<PlayerInput>> inputPlayer;
    public readonly RefRW<AimRotation> aimRotation;
    public readonly RefRW<Cooldown> cooldown;
    private readonly RefRO<GhostOwner> _ghostOwner;


    public readonly RefRO<GhostChunk> ghostChunk;


   
    public int networkId => _ghostOwner.ValueRO.NetworkId;
    public DynamicBuffer<InputBufferData<PlayerInput>> input => inputPlayer;
}

public struct CommandData<T> : IBufferElementData where T : struct, IInputComponentData
{
    public NetworkTick Tick;
    public T Command;
}