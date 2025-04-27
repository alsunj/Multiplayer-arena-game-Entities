using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public struct NetcodePlayerInput : IInputComponentData
{
    public float2 inputVector;
    public bool isSprinting;
}

public struct PlayerSprintData : IComponentData
{
    public bool isSprintCooldown;
    public float sprintRemaining;
    public float sprintDuration;
    public float sprintSpeed;
    public float walkSpeed;
    public float sprintCooldown;
    public float sprintCooldownReset;
}

public struct PlayerTag : IComponentData
{
}

public struct TeamTypes : IComponentData
{
    [GhostField] public TeamType Value;
}

public class HealthBarUIReference : ICleanupComponentData
{
    public GameObject Value;
}

public struct HealthBarOffset : IComponentData
{
    public float3 Value;
}