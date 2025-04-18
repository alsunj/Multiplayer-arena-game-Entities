using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
partial struct NetcodePlayerMovementSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetcodePlayerInput>();
        state.RequireForUpdate<PhysicsVelocity>();
        state.RequireForUpdate<LocalTransform>();
        state.RequireForUpdate<PlayerSprintData>();
        state.RequireForUpdate<GamePlayingTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach ((RefRO<NetcodePlayerInput> netcodePlayerInput, RefRW<PhysicsVelocity> physicsVelocity,
                     RefRW<LocalTransform> localTransform, RefRW<PlayerSprintData> sprintData)
                 in SystemAPI
                     .Query<RefRO<NetcodePlayerInput>, RefRW<PhysicsVelocity>, RefRW<LocalTransform>,
                         RefRW<PlayerSprintData>>()
                     .WithAll<Simulate>())
        {
            float3 moveVector = new float3(netcodePlayerInput.ValueRO.inputVector.x, 0,
                netcodePlayerInput.ValueRO.inputVector.y);

            float moveSpeed = sprintData.ValueRO.walkSpeed;

            if (netcodePlayerInput.ValueRO.isSprinting)
            {
                if (!sprintData.ValueRO.isSprintCooldown)
                {
                    sprintData.ValueRW.sprintRemaining -= deltaTime;
                    if (sprintData.ValueRW.sprintRemaining <= 0)
                    {
                        sprintData.ValueRW.isSprintCooldown = true;
                        sprintData.ValueRW.sprintCooldown = sprintData.ValueRO.sprintCooldownReset;
                    }
                    else
                    {
                        moveSpeed = sprintData.ValueRO.sprintSpeed;
                    }
                }
            }

            if (sprintData.ValueRO.isSprintCooldown)
            {
                sprintData.ValueRW.sprintCooldown -= deltaTime;
                if (sprintData.ValueRW.sprintCooldown <= 0)
                {
                    sprintData.ValueRW.isSprintCooldown = false;
                }
            }


            physicsVelocity.ValueRW.Linear =
                math.lerp(physicsVelocity.ValueRO.Linear, moveVector * moveSpeed, deltaTime * 10);

            if (!math.all(moveVector == float3.zero))
            {
                quaternion targetRotation = quaternion.LookRotationSafe(moveVector, math.up());
                localTransform.ValueRW.Rotation =
                    math.slerp(localTransform.ValueRO.Rotation, targetRotation, deltaTime * 10);
            }
        }
    }
}