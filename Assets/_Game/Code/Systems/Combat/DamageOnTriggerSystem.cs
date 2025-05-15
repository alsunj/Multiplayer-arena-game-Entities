using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

/// <summary>
/// System responsible for handling damage when trigger events occur between entities, applies the damage on DamageBufferElement buffer.
/// This system runs within the PhysicsSystemGroup and executes after the PhysicsSimulationGroup.
/// </summary>
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct DamageOnTriggerSystem : ISystem
{
    /// <summary>
    /// Initializes the system by specifying the required components for it to update.
    /// Ensures that the system only runs when the SimulationSingleton, EndSimulationEntityCommandBufferSystem.Singleton,
    /// and GamePlayingTag components are present.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<GamePlayingTag>();
    }

    /// <summary>
    /// Executes the system logic every frame. Schedules the DamageOnTriggerJob to process trigger events
    /// and apply damage to entities based on their interactions.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Retrieve the singleton for the EndSimulationEntityCommandBufferSystem.
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

        // Create and configure the DamageOnTriggerJob.
        var damageOnTriggerJob = new DamageOnTriggerJob
        {
            DamageOnTriggerLookup = SystemAPI.GetComponentLookup<DamageOnTrigger>(true),
            AlreadyDamagedLookup = SystemAPI.GetBufferLookup<AlreadyDamagedEntity>(true),
            DamageBufferLookup = SystemAPI.GetBufferLookup<DamageBufferElement>(true),
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged)
        };

        // Retrieve the SimulationSingleton and schedule the job.
        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        state.Dependency = damageOnTriggerJob.Schedule(simulationSingleton, state.Dependency);
    }
}

/// <summary>
/// Job that processes trigger events and applies damage to entities based on their interactions.
/// </summary>
public struct DamageOnTriggerJob : ITriggerEventsJob
{
    /// <summary>
    /// Read-only lookup for DamageOnTrigger components to determine damage-dealing entities.
    /// </summary>
    [ReadOnly] public ComponentLookup<DamageOnTrigger> DamageOnTriggerLookup;

    /// <summary>
    /// Lookup for AlreadyDamagedEntity buffers to track entities that have already been damaged.
    /// </summary>
    public BufferLookup<AlreadyDamagedEntity> AlreadyDamagedLookup;

    /// <summary>
    /// Lookup for DamageBufferElement buffers to store damage values for entities.
    /// </summary>
    public BufferLookup<DamageBufferElement> DamageBufferLookup;

    /// <summary>
    /// EntityCommandBuffer used to queue entity modifications during the job execution.
    /// </summary>
    public EntityCommandBuffer ECB;

    /// <summary>
    /// Executes the job for each trigger event. Determines the damage-dealing and damage-receiving entities,
    /// applies damage, and marks entities for destruction if necessary.
    /// </summary>
    /// <param name="triggerEvent">The trigger event containing the interacting entities.</param>
    public void Execute(TriggerEvent triggerEvent)
    {
        Entity entityA = triggerEvent.EntityA;
        Entity entityB = triggerEvent.EntityB;
        Entity damageDealingEntity = Entity.Null;
        Entity damageReceivingEntity = Entity.Null;

        // Determine which entity is dealing damage and which is receiving damage.
        if (DamageBufferLookup.HasBuffer(entityA) &&
            DamageOnTriggerLookup.HasComponent(entityB))
        {
            damageReceivingEntity = entityA;
            damageDealingEntity = entityB;
        }
        else if (DamageOnTriggerLookup.HasComponent(entityA) &&
                 DamageBufferLookup.HasBuffer(entityB))
        {
            damageDealingEntity = entityA;
            damageReceivingEntity = entityB;
        }
        else
        {
            return;
        }

        // Check if the damage-dealing entity has already damaged the receiving entity.
        if (AlreadyDamagedLookup.HasBuffer(damageDealingEntity))
        {
            var alreadyDamagedBuffer = AlreadyDamagedLookup[damageDealingEntity];
            for (int i = 0; i < alreadyDamagedBuffer.Length; i++)
            {
                if (alreadyDamagedBuffer[i].Value == damageReceivingEntity) return;
            }
        }
        else
        {
            // Add a buffer to track already damaged entities if it doesn't exist.
            ECB.AddBuffer<AlreadyDamagedEntity>(damageDealingEntity);
        }

        // Apply damage and mark the damage-dealing entity for destruction if applicable.
        if (DamageOnTriggerLookup.TryGetComponent(damageDealingEntity, out var damageOnTrigger))
        {
            ECB.AddComponent<DestroyEntityTag>(damageDealingEntity);
            ECB.AppendToBuffer(damageReceivingEntity, new DamageBufferElement { Value = damageOnTrigger.Value });
            ECB.AppendToBuffer(damageDealingEntity, new AlreadyDamagedEntity { Value = damageReceivingEntity });
        }
    }
}