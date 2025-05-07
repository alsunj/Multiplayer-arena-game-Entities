using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
public partial struct MoveSlimeSystem : ISystem
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
        var transformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

        state.Dependency = new SlimeMoveDirectJob
        {
            TransformLookup = transformLookup
        }.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    public partial struct SlimeMoveDirectJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;

        [BurstCompile]
        private void Execute(
            ref PhysicsVelocity velocity,
            in LocalTransform transform,
            in AbilityMoveSpeed moveSpeed,
            in NpcTargetEntity targetEntity)
        {
            if (targetEntity.Value == Entity.Null || !TransformLookup.HasComponent(targetEntity.Value))
            {
                velocity.Linear = float3.zero;
                return;
            }

            var targetPosition = TransformLookup[targetEntity.Value].Position;
            var direction = math.normalize(targetPosition - transform.Position);
            velocity.Linear = direction * moveSpeed.Value;
        }
    }
}