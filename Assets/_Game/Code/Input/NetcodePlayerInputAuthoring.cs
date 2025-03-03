using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class NetcodePlayerInputAuthoring : MonoBehaviour
{
    public class Baker : Baker<NetcodePlayerInputAuthoring>
    {
        public override void Bake(NetcodePlayerInputAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new NetcodePlayerInput());

            // Assign initial values to PlayerSprintData
            AddComponent(entity, new PlayerSprintData
            {
                isSprinting = false,
                isSprintCooldown = false,
                sprintRemaining = 5f,
                sprintDuration = 5f,
                sprintSpeed = 12f,
                walkSpeed = 9f,
                sprintCooldown = 0f,
                sprintCooldownReset = 2f,
                sprintFOV = 90f,
                walkFOV = 60f,
                sprintFOVStepTime = 0.1f
            });

            AddComponent(entity, new PlayerAttackData());
            AddComponent(entity, new PlayerDefenceData());
            AddComponent(entity, new CameraFollow());
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

public struct CameraFollow : IComponentData
{
    public Entity PlayerEntity;
}