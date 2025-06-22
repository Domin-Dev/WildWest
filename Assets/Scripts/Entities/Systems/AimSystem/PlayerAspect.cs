using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;




public readonly partial struct PlayerAspect : IAspect
{
    private readonly DynamicBuffer<CooldownTargetTick> _cooldownTargetTick;
    public readonly RefRW<PlayerInput> playerInputSync;
  //  public readonly RefRW<PlayerInput> playerInput;
    public readonly RefRW<Hands> hands;
    public readonly RefRW<Character> character;
    public readonly RefRW<Player> player;

    private readonly RefRO<GhostOwner> _ghostOwner;

    public int networkId => _ghostOwner.ValueRO.NetworkId;
    public DynamicBuffer<CooldownTargetTick> cooldownTargetTick => _cooldownTargetTick;
}
