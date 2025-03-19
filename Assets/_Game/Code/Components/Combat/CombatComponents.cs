using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public struct NpcTargetRadius : IComponentData
{
    public float Value;
}

public struct NpcTargetEntity : IComponentData
{
    [GhostField] public Entity Value;
}

public struct NpcAttackProperties : IComponentData
{
    public float3 FirePointOffset;
    public uint CooldownTickCount;
    public Entity AttackPrefab;
}

public struct NpcAttackCooldown : ICommandData
{
    public NetworkTick Tick { get; set; }
    public NetworkTick Value;
}

public struct DestroyOnTimer : IComponentData
{
    public float Value;
}

public struct DestroyAtTick : IComponentData
{
    [GhostField] public NetworkTick Value;
}

public struct DestroyEntityTag : IComponentData
{
}


public struct AbilityMoveSpeed : IComponentData
{
    public float Value;
}

public struct DamageOnTrigger : IComponentData
{
    public int Value;
}

public struct AlreadyDamagedEntity : IBufferElementData
{
    public Entity Value;
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted)]
public struct DamageBufferElement : IBufferElementData
{
    public int Value;
}

[GhostComponent(PrefabType = GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToNonOwner)]
public struct DamageThisTick : ICommandData
{
    public NetworkTick Tick { get; set; }
    public int Value;
}

public struct MaxHitPoints : IComponentData
{
    public int Value;
}

public struct CurrentHitPoints : IComponentData
{
    [GhostField] public int Value;
}