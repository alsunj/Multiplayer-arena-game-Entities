using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// System responsible for calculating the total damage for the current frame.
/// This system runs in the PredictedSimulationSystemGroup and executes last in the group.
/// </summary>
[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
public partial struct CalculateFrameDamageSystem : ISystem
{
    /// <summary>
    /// Called when the system is created. Ensures the system only updates when
    /// the required components (NetworkTime, GamePlayingTag, and DamageBufferElement) are present.
    /// </summary>
    /// <param name="state">The system state.</param>
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<GamePlayingTag>();
        state.RequireForUpdate<DamageBufferElement>();
    }

    /// <summary>
    /// Called every frame to update the system. Aggregates damage from the DamageBufferElement
    /// and stores the total damage for the current tick in the DamageThisTick buffer.
    /// Clears the damage buffer after processing.
    /// </summary>
    /// <param name="state">The system state.</param>
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Retrieve the current server tick from the NetworkTime singleton.
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        // Iterate over entities with DamageBufferElement and DamageThisTick buffers, and the Simulate tag.
        foreach (var (damageBuffer, damageThisTickBuffer) in SystemAPI
                     .Query<DynamicBuffer<DamageBufferElement>, DynamicBuffer<DamageThisTick>>()
                     .WithAll<Simulate>())
        {
            // If the damage buffer is empty, add a zero-damage entry for the current tick.
            if (damageBuffer.IsEmpty)
            {
                damageThisTickBuffer.AddCommandData(new DamageThisTick { Tick = currentTick, Value = 0 });
            }
            else
            {
                // Calculate the total damage for the current tick.
                var totalDamage = 0;
                if (damageThisTickBuffer.GetDataAtTick(currentTick, out var damageThisTick))
                {
                    totalDamage = damageThisTick.Value;
                }

                // Add up all damage values from the damage buffer.
                foreach (var damage in damageBuffer)
                {
                    totalDamage += damage.Value;
                }

                // Store the total damage in the DamageThisTick buffer and clear the damage buffer.
                damageThisTickBuffer.AddCommandData(new DamageThisTick { Tick = currentTick, Value = totalDamage });
                damageBuffer.Clear();
            }
        }
    }
}