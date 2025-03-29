using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct MoveSlimeSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AbilityMoveSpeed>();
        state.RequireForUpdate<TeamTypes>();
        state.RequireForUpdate<NpcTargetEntity>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);

        foreach (var (transform, moveSpeed, teamType, targetEntity) in SystemAPI
                     .Query<RefRW<LocalTransform>, RefRO<AbilityMoveSpeed>, RefRO<TeamTypes>, RefRO<NpcTargetEntity>>()
                     .WithAny<SlimeTag>())
        {
            if (teamType.ValueRO.Value == TeamType.Enemy && targetEntity.ValueRO.Value != Entity.Null)
            {
                if (localTransformLookup.HasComponent(targetEntity.ValueRO.Value))
                {
                    var targetPosition = localTransformLookup[targetEntity.ValueRO.Value].Position;
                    var direction = math.normalize(targetPosition - transform.ValueRW.Position);
                    transform.ValueRW.Position += direction * moveSpeed.ValueRO.Value * deltaTime;
                }
            }
        }
    }
}