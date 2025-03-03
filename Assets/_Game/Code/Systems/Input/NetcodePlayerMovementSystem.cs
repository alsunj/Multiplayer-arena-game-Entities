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
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;

        foreach ((RefRO<NetcodePlayerInput> netcodePlayerInput, RefRW<PhysicsVelocity> physicsVelocity,
                     RefRW<LocalTransform> localTransform, RefRW<PlayerSprintData> sprintData,
                     RefRO<CameraFollow> cameraFollow)
                 in SystemAPI
                     .Query<RefRO<NetcodePlayerInput>, RefRW<PhysicsVelocity>, RefRW<LocalTransform>,
                         RefRW<PlayerSprintData>, RefRO<CameraFollow>>()
                     .WithAll<Simulate>())
        {
            Debug.Log("Updating player movement and camera follow");

            float3 moveVector = new float3(netcodePlayerInput.ValueRO.inputVector.x, 0,
                netcodePlayerInput.ValueRO.inputVector.y);
            float moveSpeed = sprintData.ValueRO.isSprinting
                ? sprintData.ValueRO.sprintSpeed
                : sprintData.ValueRO.walkSpeed;

            // Update sprinting logic
            if (sprintData.ValueRO.isSprinting && !sprintData.ValueRO.isSprintCooldown)
            {
                sprintData.ValueRW.sprintRemaining -= deltaTime;
                if (sprintData.ValueRW.sprintRemaining <= 0)
                {
                    sprintData.ValueRW.isSprinting = false;
                    sprintData.ValueRW.isSprintCooldown = true;
                }
            }
            else
            {
                sprintData.ValueRW.sprintRemaining = math.clamp(sprintData.ValueRW.sprintRemaining + deltaTime, 0,
                    sprintData.ValueRO.sprintDuration);
            }

            if (sprintData.ValueRO.isSprintCooldown)
            {
                sprintData.ValueRW.sprintCooldown -= deltaTime;
                if (sprintData.ValueRW.sprintCooldown <= 0)
                {
                    sprintData.ValueRW.isSprintCooldown = false;
                    sprintData.ValueRW.sprintCooldown = sprintData.ValueRO.sprintCooldownReset;
                }
            }

            // Apply instant movement
            physicsVelocity.ValueRW.Linear = moveVector * moveSpeed;

            // Optionally, update the rotation to face the movement direction
            if (!math.all(moveVector == float3.zero))
            {
                quaternion targetRotation = quaternion.LookRotationSafe(moveVector, math.up());
                localTransform.ValueRW.Rotation = math.slerp(localTransform.ValueRO.Rotation, targetRotation, 0.1f);
            }

            // Update camera position and FOV
            Entity playerEntity = cameraFollow.ValueRO.PlayerEntity;
            if (SystemAPI.HasComponent<LocalTransform>(playerEntity))
            {
                LocalTransform playerTransform = SystemAPI.GetComponent<LocalTransform>(playerEntity);
                float3 cameraPosition = playerTransform.Position + new float3(0, 2, -5);
                SystemAPI.SetComponent(cameraFollow.ValueRO.PlayerEntity,
                    new LocalTransform { Position = cameraPosition });

                // Update camera FOV
                Camera.main.fieldOfView = math.lerp(Camera.main.fieldOfView,
                    sprintData.ValueRO.isSprinting ? sprintData.ValueRO.sprintFOV : sprintData.ValueRO.walkFOV,
                    sprintData.ValueRO.sprintFOVStepTime * deltaTime);
            }
        }
    }
}