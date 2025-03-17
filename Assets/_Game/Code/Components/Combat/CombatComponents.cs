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