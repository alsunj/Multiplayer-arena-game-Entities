using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// System responsible for handling player movement in a netcode environment.
/// This system processes player input, updates physics velocity, and adjusts player rotation
/// based on movement direction and sprinting state.
/// </summary>
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
partial struct NetcodePlayerMovementSystem : ISystem
{
    /// <summary>
    /// Called when the system is created. Ensures the system only updates when the required components
    /// and entities are present in the world.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Require specific components and entities for the system to update.
        state.RequireForUpdate<NetcodePlayerInput>();
        state.RequireForUpdate<PhysicsVelocity>();
        state.RequireForUpdate<LocalTransform>();
        state.RequireForUpdate<PlayerSprintData>();
        state.RequireForUpdate<GamePlayingTag>();
    }

    /// <summary>
    /// Called every frame to process player movement logic.
    /// Updates the player's velocity and rotation based on input and sprinting state.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Retrieve the time elapsed since the last frame.
        float deltaTime = SystemAPI.Time.DeltaTime;

        // Iterate through all entities with the required components.
        foreach ((RefRO<NetcodePlayerInput> netcodePlayerInput, RefRW<PhysicsVelocity> physicsVelocity,
                     RefRW<LocalTransform> localTransform, RefRW<PlayerSprintData> sprintData)
                 in SystemAPI
                     .Query<RefRO<NetcodePlayerInput>, RefRW<PhysicsVelocity>, RefRW<LocalTransform>,
                         RefRW<PlayerSprintData>>()
                     .WithAll<Simulate>())
        {
            // Calculate the movement vector based on player input.
            float3 moveVector = new float3(netcodePlayerInput.ValueRO.inputVector.x, 0,
                netcodePlayerInput.ValueRO.inputVector.y);

            // Set the default movement speed to walking speed.
            float moveSpeed = sprintData.ValueRO.walkSpeed;

            // Check if the player is sprinting.
            if (netcodePlayerInput.ValueRO.isSprinting)
            {
                // Handle sprinting logic and cooldowns.
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

            // Handle sprint cooldown logic.
            if (sprintData.ValueRO.isSprintCooldown)
            {
                sprintData.ValueRW.sprintCooldown -= deltaTime;
                if (sprintData.ValueRW.sprintCooldown <= 0)
                {
                    sprintData.ValueRW.isSprintCooldown = false;
                }
            }

            // Smoothly update the player's velocity based on the movement vector and speed.
            physicsVelocity.ValueRW.Linear =
                math.lerp(physicsVelocity.ValueRO.Linear, moveVector * moveSpeed, deltaTime * 10);

            // Update the player's rotation to face the movement direction if moving.
            if (!math.all(moveVector == float3.zero))
            {
                quaternion targetRotation = quaternion.LookRotationSafe(moveVector, math.up());
                localTransform.ValueRW.Rotation =
                    math.slerp(localTransform.ValueRO.Rotation, targetRotation, deltaTime * 10);
            }
        }
    }
}