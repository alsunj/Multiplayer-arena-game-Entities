using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Unity.NetCode;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct MoveAbilitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GamePlayingTag>(); 
        state.RequireForUpdate<AbilityMoveSpeed>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new MoveAbilityJob()
            .ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithNone(typeof(SlimeTag))]
public partial struct MoveAbilityJob : IJobEntity
{
    [BurstCompile]
    private void Execute(
        ref PhysicsVelocity velocity,
        in LocalTransform transform,
        in AbilityMoveSpeed moveSpeed)
    {
        velocity.Linear = transform.Forward() * moveSpeed.Value;
    }
}