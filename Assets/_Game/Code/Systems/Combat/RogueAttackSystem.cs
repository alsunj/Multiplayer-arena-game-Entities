using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

/// <summary>
/// System responsible for handling rogue NPC attacks. This system ensures that NPCs can attack their targets
/// based on cooldowns and attack properties, and updates their transformations accordingly.
/// </summary>
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct RogueAttackSystem : ISystem
{
    /// <summary>
    /// Called when the system is created. Ensures the system only updates when the required components
    /// (NetworkTime, BeginSimulationEntityCommandBufferSystem.Singleton, and GamePlayingTag) are present.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<GamePlayingTag>();
    }

    /// <summary>
    /// Called every frame to update the system. Schedules the NpcAttackJob to process NPC attacks in parallel.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnUpdate(ref SystemState state)
    {
        // Retrieve the EntityCommandBuffer singleton and the current network time.
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();

        // Schedule the NpcAttackJob to handle NPC attack logic.
        state.Dependency = new NpcAttackJob
        {
            CurrentTick = networkTime.ServerTick,
            TransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
        }.ScheduleParallel(state.Dependency);
    }
}

/// <summary>
/// Job that processes NPC attack logic. Handles attack cooldowns, target rotation, and attack instantiation.
/// Excludes entities with the SlimeTag component.
/// </summary>
[BurstCompile]
[WithAll(typeof(Simulate))]
[WithNone(typeof(SlimeTag))]
public partial struct NpcAttackJob : IJobEntity
{
    /// <summary>
    /// The current server tick used to determine attack cooldowns.
    /// </summary>
    [ReadOnly] public NetworkTick CurrentTick;

    /// <summary>
    /// Read-only lookup for LocalTransform components to access entity positions and rotations.
    /// </summary>
    [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;

    /// <summary>
    /// Parallel writer for queuing entity modifications during the job execution.
    /// </summary>
    public EntityCommandBuffer.ParallelWriter ECB;

    /// <summary>
    /// Executes the job for each entity. Handles attack cooldowns, rotates the NPC towards its target,
    /// and spawns attack entities with the appropriate transformations.
    /// </summary>
    /// <param name="attackCooldown">The buffer storing attack cooldown data for the NPC.</param>
    /// <param name="attackProperties">The properties defining the NPC's attack behavior.</param>
    /// <param name="targetEntity">The target entity the NPC is attacking.</param>
    /// <param name="enemyEntity">The NPC entity performing the attack.</param>
    /// <param name="team">The team type of the NPC.</param>
    /// <param name="sortKey">The chunk index used for parallel execution.</param>
    [BurstCompile]
    private void Execute(
        ref DynamicBuffer<NpcAttackCooldown> attackCooldown,
        in NpcAttackProperties attackProperties,
        in NpcTargetEntity targetEntity,
        Entity enemyEntity,
        TeamTypes team,
        [ChunkIndexInQuery] int sortKey)
    {
        // Ensure the target entity has a LocalTransform component.
        if (!TransformLookup.HasComponent(targetEntity.Value)) return;

        // Retrieve or initialize the cooldown expiration tick.
        if (!attackCooldown.GetDataAtTick(CurrentTick, out var cooldownExpirationTick))
        {
            cooldownExpirationTick.Value = NetworkTick.Invalid;
        }

        // Check if the NPC can attack based on the cooldown.
        var canAttack = !cooldownExpirationTick.Value.IsValid ||
                        CurrentTick.IsNewerThan(cooldownExpirationTick.Value);
        if (!canAttack) return;

        // Retrieve the enemy's and target's transformations.
        var enemyTransform = TransformLookup[enemyEntity];
        var targetPosition = TransformLookup[targetEntity.Value].Position;

        // Calculate the direction and rotation towards the target.
        var direction = math.normalize(targetPosition - enemyTransform.Position);
        var targetRotation = quaternion.LookRotationSafe(direction, math.up());

        // Rotate the NPC towards the target.
        enemyTransform.Rotation = targetRotation;
        ECB.SetComponent(sortKey, enemyEntity, enemyTransform);

        // Spawn the attack entity at the appropriate position and rotation.
        var newAttack = ECB.Instantiate(sortKey, attackProperties.AttackPrefab);
        var newAttackTransform = LocalTransform.FromPositionRotation(
            enemyTransform.Position + attackProperties.FirePointOffset,
            quaternion.LookRotationSafe(targetPosition - enemyTransform.Position, math.up()));

        ECB.SetComponent(sortKey, newAttack, newAttackTransform);
        ECB.SetComponent(sortKey, newAttack, team);

        // Update the attack cooldown.
        var newCooldownTick = CurrentTick;
        newCooldownTick.Add(attackProperties.CooldownTickCount);
        attackCooldown.AddCommandData(new NpcAttackCooldown { Tick = CurrentTick, Value = newCooldownTick });
    }
}