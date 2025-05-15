using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(CalculateFrameDamageSystem))]
public partial struct ApplyDamageSystem : ISystem
{
    /// <summary>
    /// Called when the system is created. Ensures the system only updates when
    /// the required components (NetworkTime and GamePlayingTag) are present.
    /// </summary>
    /// <param name="state">The system state.</param>
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<GamePlayingTag>();
    }

    /// <summary>
    /// Called every frame to update the system. Processes damage for entities
    /// with a DamageThisTick buffer and applies it to their CurrentHitPoints.
    /// If an entity's hit points drop to zero or below, it is marked for destruction.
    /// </summary>
    /// <param name="state">The system state.</param>
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Get the current server tick from the NetworkTime singleton.
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        // Create a temporary EntityCommandBuffer to queue entity modifications.
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Query entities with CurrentHitPoints, DamageThisTick buffer, and Simulate tag.
        foreach (var (currentHitPoints, damageThisTickBuffer, entity) in SystemAPI
                     .Query<RefRW<CurrentHitPoints>, DynamicBuffer<DamageThisTick>>().WithAll<Simulate>()
                     .WithEntityAccess())
        {
            // Skip if no damage data exists for the current tick.
            if (!damageThisTickBuffer.GetDataAtTick(currentTick, out var damageThisTick)) continue;
            if (damageThisTick.Tick != currentTick) continue;

            // Apply the damage to the entity's current hit points.
            currentHitPoints.ValueRW.Value -= damageThisTick.Value;

            // If hit points drop to zero or below, mark the entity for destruction.
            if (currentHitPoints.ValueRO.Value <= 0)
            {
                ecb.AddComponent<DestroyEntityTag>(entity);
            }
        }

        // Apply all queued entity modifications.
        ecb.Playback(state.EntityManager);
    }
}