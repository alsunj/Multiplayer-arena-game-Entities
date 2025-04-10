using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
[UpdateBefore(typeof(MoveSlimeSystem))]
public partial struct SetTargetSlimeSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<SlimeTag>();
        state.RequireForUpdate<GamePlayingTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

        var teamLookup = SystemAPI.GetComponentLookup<TeamTypes>(true);
        var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

        state.Dependency = new SetSlimeTargetDirectionJob
        {
            TeamLookup = teamLookup,
            TransformLookup = transformLookup,
            Ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()
        }.ScheduleParallel(state.Dependency);
    }
}


[BurstCompile]
public partial struct SetSlimeTargetDirectionJob : IJobEntity
{
    [ReadOnly] public ComponentLookup<TeamTypes> TeamLookup;
    [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
    public EntityCommandBuffer.ParallelWriter Ecb;

    [BurstCompile]
    private void Execute(in LocalTransform transform, in TeamTypes slimeTeam, in NpcTargetEntity targetEntity,
        in Entity entity)
    {
        if (targetEntity.Value == Entity.Null || !TransformLookup.HasComponent(targetEntity.Value) ||
            !TeamLookup.HasComponent(targetEntity.Value))
        {
            Ecb.RemoveComponent<SlimeTargetDirection>(entity.Index, entity);
            return;
        }

        var targetTeam = TeamLookup[targetEntity.Value].Value;

        if (targetTeam != TeamType.Enemy)
        {
            var targetPosition = TransformLookup[targetEntity.Value].Position;
            var direction = math.normalize(targetPosition - transform.Position);
            Ecb.AddComponent(entity.Index, entity, new SlimeTargetDirection { Value = direction });
        }
        else
        {
            Ecb.RemoveComponent<SlimeTargetDirection>(entity.Index, entity);
        }
    }
}