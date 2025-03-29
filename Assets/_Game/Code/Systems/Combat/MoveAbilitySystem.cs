using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct MoveAbilitySystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        state.Dependency = new MoveAbilityJob
        {
            DeltaTime = deltaTime
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithNone(typeof(SlimeTag))]
public partial struct MoveAbilityJob : IJobEntity
{
    public float DeltaTime;

    [BurstCompile]
    private void Execute(ref LocalTransform transform, in AbilityMoveSpeed moveSpeed)
    {
        transform.Position += transform.Forward() * moveSpeed.Value * DeltaTime;
    }
}