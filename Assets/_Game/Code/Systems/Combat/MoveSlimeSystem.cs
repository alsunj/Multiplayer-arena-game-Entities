using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct MoveSlimeSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<SlimeTargetDirection>();
        state.RequireForUpdate<SlimeTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        state.Dependency = new MoveSlimeJob
        {
            DeltaTime = deltaTime
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(SlimeTargetDirection))]
public partial struct MoveSlimeJob : IJobEntity
{
    public float DeltaTime;

    [BurstCompile]
    private void Execute(ref LocalTransform transform, in AbilityMoveSpeed moveSpeed,
        in SlimeTargetDirection targetDirection)
    {
        transform.Position += targetDirection.Value * moveSpeed.Value * DeltaTime;
    }
}