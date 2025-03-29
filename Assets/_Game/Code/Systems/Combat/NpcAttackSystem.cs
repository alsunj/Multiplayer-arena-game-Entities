using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct NpcAttackSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();

        state.Dependency = new NpcAttackJob
        {
            CurrentTick = networkTime.ServerTick,
            TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(Simulate))]
[WithNone(typeof(SlimeTag))]
public partial struct NpcAttackJob : IJobEntity
{
    [ReadOnly] public NetworkTick CurrentTick;
    [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
    public EntityCommandBuffer.ParallelWriter ECB;

    [BurstCompile]
    private void Execute(ref DynamicBuffer<NpcAttackCooldown> attackCooldown, in NpcAttackProperties attackProperties,
        in NpcTargetEntity targetEntity, Entity enemyEntity, TeamTypes team, [ChunkIndexInQuery] int sortKey)
    {
        if (!TransformLookup.HasComponent(targetEntity.Value)) return;
        if (!attackCooldown.GetDataAtTick(CurrentTick, out var cooldownExpirationTick))
        {
            cooldownExpirationTick.Value = NetworkTick.Invalid;
        }

        var canAttack = !cooldownExpirationTick.Value.IsValid ||
                        CurrentTick.IsNewerThan(cooldownExpirationTick.Value);
        if (!canAttack) return;


        var enemyTransform = TransformLookup[enemyEntity];

        var spawnPosition = enemyTransform.Position;
        var targetPosition = TransformLookup[targetEntity.Value].Position;

        // Get the vector from the enemy to the player
        var direction = math.normalize(targetPosition - spawnPosition);
        var targetRotation = quaternion.LookRotationSafe(direction, math.up());

        // Rotate the enemy towards player
        enemyTransform.Rotation = targetRotation;

        ECB.SetComponent(sortKey, enemyEntity, enemyTransform);

        var newAttack = ECB.Instantiate(sortKey, attackProperties.AttackPrefab);
        var newAttackTransform = LocalTransform.FromPositionRotation(spawnPosition + attackProperties.FirePointOffset,
            quaternion.LookRotationSafe(targetPosition - spawnPosition, math.up()));

        ECB.SetComponent(sortKey, newAttack, newAttackTransform);
        ECB.SetComponent(sortKey, newAttack, team);

        var newCooldownTick = CurrentTick;
        newCooldownTick.Add(attackProperties.CooldownTickCount);
        attackCooldown.AddCommandData(new NpcAttackCooldown { Tick = CurrentTick, Value = newCooldownTick });
    }
}