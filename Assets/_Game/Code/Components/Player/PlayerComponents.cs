using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public struct NetcodePlayerInput : IInputComponentData
{
    public float2 inputVector;
}

public struct PlayerSprintData : IComponentData
{
    public bool isSprinting;
    public bool isSprintCooldown;
    public float sprintRemaining;
    public float sprintDuration;
    public float sprintSpeed;
    public float walkSpeed;
    public float sprintCooldown;
    public float sprintCooldownReset;
}

public struct PlayerAttackData : IComponentData
{
    public float attackCooldownTimer;
}

public struct PlayerDefenceData : IComponentData
{
    public float defenceCooldownTimer;
}

public struct PlayerTag : IComponentData
{
}


public struct TeamTypes : IComponentData
{
    [GhostField] public TeamType Value;
}