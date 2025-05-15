using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

/// <summary>
/// System responsible for moving slime entities towards their target positions.
/// This system runs in the PredictedSimulationSystemGroup and executes last in the group.
/// </summary>
[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
public partial struct MoveSlimeSystem : ISystem
{
    /// <summary>
    /// Called when the system is created. Ensures the system only updates when
    /// entities with the required components (SlimeTag and GamePlayingTag) exist.
    /// </summary>
    /// <param name="state">The system state.</param>
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SlimeTag>();
        state.RequireForUpdate<GamePlayingTag>();
    }

    /// <summary>
    /// Called every frame to update the system. Schedules a job to move slime entities
    /// towards their target positions based on their move speed and target entity.
    /// </summary>
    /// <param name="state">The system state.</param>
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Lookup for LocalTransform components, used to get the position of target entities.
        var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

        // Schedule the SlimeMoveDirectJob to run in parallel.
        state.Dependency = new SlimeMoveDirectJob
        {
            TransformLookup = transformLookup
        }.ScheduleParallel(state.Dependency);
    }

    /// <summary>
    /// Job that handles the movement of slime entities towards their target positions.
    /// </summary>
    [BurstCompile]
    public partial struct SlimeMoveDirectJob : IJobEntity
    {
        /// <summary>
        /// Read-only lookup for LocalTransform components to access target entity positions.
        /// </summary>
        [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;

        /// <summary>
        /// Executes the job for each entity. Updates the entity's velocity to move it
        /// towards its target position, or stops it if no valid target exists.
        /// </summary>
        /// <param name="velocity">The entity's current velocity.</param>
        /// <param name="transform">The entity's current transform.</param>
        /// <param name="moveSpeed">The entity's movement speed.</param>
        /// <param name="targetEntity">The target entity the slime is moving towards.</param>
        [BurstCompile]
        private void Execute(
            ref PhysicsVelocity velocity,
            in LocalTransform transform,
            in AbilityMoveSpeed moveSpeed,
            in NpcTargetEntity targetEntity)
        {
            // If the target entity is null or does not have a LocalTransform, stop the entity.
            if (targetEntity.Value == Entity.Null || !TransformLookup.HasComponent(targetEntity.Value))
            {
                velocity.Linear = float3.zero;
                return;
            }

            // Calculate the direction towards the target and update the velocity.
            var targetPosition = TransformLookup[targetEntity.Value].Position;
            var direction = math.normalize(targetPosition - transform.Position);
            velocity.Linear = direction * moveSpeed.Value;
        }
    }
}