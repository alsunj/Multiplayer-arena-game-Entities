using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class NetcodePlayerInputAuthoring : MonoBehaviour
{
    public float sprintRemaining = 5f;
    public float sprintDuration = 5f;
    public float sprintSpeed = 12f;
    public float walkSpeed = 9f;
    public float sprintCooldownReset = 2f;
    public float sprintFOV = 90f;
    public float walkFOV = 60f;
    public float sprintFOVStepTime = 0.1f;

    public class Baker : Baker<NetcodePlayerInputAuthoring>
    {
        public override void Bake(NetcodePlayerInputAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new NetcodePlayerInput());

            AddComponent(entity, new PlayerSprintData
            {
                isSprinting = false,
                isSprintCooldown = false,
                sprintRemaining = authoring.sprintRemaining,
                sprintDuration = authoring.sprintDuration,
                sprintSpeed = authoring.sprintSpeed,
                walkSpeed = authoring.walkSpeed,
                sprintCooldown = 0f,
                sprintCooldownReset = authoring.sprintCooldownReset,
                sprintFOV = authoring.sprintFOV,
                walkFOV = authoring.walkFOV,
                sprintFOVStepTime = authoring.sprintFOVStepTime
            });

            AddComponent(entity, new PlayerAttackData());
            AddComponent(entity, new PlayerDefenceData());
        }
    }
}

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
    public float sprintFOV;
    public float walkFOV;
    public float sprintFOVStepTime;
}

public struct PlayerAttackData : IComponentData
{
    public float attackCooldownTimer;
}

public struct PlayerDefenceData : IComponentData
{
    public float defenceCooldownTimer;
}