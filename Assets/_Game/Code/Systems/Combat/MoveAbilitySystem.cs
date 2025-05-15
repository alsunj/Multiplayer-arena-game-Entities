using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.NetCode;

/// <summary>
/// System responsible for handling movement abilities of entities. 
/// Updates the velocity of entities based on their movement speed and direction.
/// </summary>
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct MoveAbilitySystem : ISystem
{
    /// <summary>
    /// Called when the system is created. Ensures the system only updates when the required components
    /// (GamePlayingTag and AbilityMoveSpeed) are present.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GamePlayingTag>();
        state.RequireForUpdate<AbilityMoveSpeed>();
    }

    /// <summary>
    /// Called every frame to update the system. Schedules the MoveAbilityJob to process entity movement in parallel.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new MoveAbilityJob()
            .ScheduleParallel(state.Dependency);
    }
}

/// <summary>
/// Job that processes entity movement by updating their velocity based on their forward direction
/// and movement speed. Excludes entities with the SlimeTag component.
/// </summary>
[BurstCompile]
[WithNone(typeof(SlimeTag))]
public partial struct MoveAbilityJob : IJobEntity
{
    /// <summary>
    /// Executes the job for each entity. Calculates the linear velocity of the entity
    /// based on its forward direction and movement speed.
    /// </summary>
    /// <param name="velocity">The physics velocity of the entity.</param>
    /// <param name="transform">The local transform of the entity.</param>
    /// <param name="moveSpeed">The movement speed of the entity.</param>
    [BurstCompile]
    private void Execute(
        ref PhysicsVelocity velocity,
        in LocalTransform transform,
        in AbilityMoveSpeed moveSpeed)
    {
        velocity.Linear = transform.Forward() * moveSpeed.Value;
    }
}