using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// System responsible for initializing the destruction timer for entities. 
/// This system calculates the tick at which entities should be destroyed based on their lifetime
/// and adds a `DestroyAtTick` component to mark the destruction time.
/// </summary>
public partial struct InitializeDestroyOnTimerSystem : ISystem
{
    /// <summary>
    /// Called when the system is created. Ensures the system only updates when the required components
    /// and entities are present in the world.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnCreate(ref SystemState state)
    {
        // Require specific components and entities for the system to update.
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<DestroyOnTimer>();
        state.RequireForUpdate<GamePlayingTag>();
    }

    /// <summary>
    /// Called every frame to process entities with a `DestroyOnTimer` component. Calculates the destruction tick
    /// based on the entity's lifetime and adds a `DestroyAtTick` component to schedule its destruction.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnUpdate(ref SystemState state)
    {
        // Create a temporary EntityCommandBuffer to queue entity modifications.
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Retrieve the simulation tick rate and the current server tick.
        var simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        // Iterate through all entities with a `DestroyOnTimer` component but without a `DestroyAtTick` component.
        foreach (var (destroyOnTimer, entity) in SystemAPI.Query<RefRW<DestroyOnTimer>>().WithNone<DestroyAtTick>()
                     .WithEntityAccess())
        {
            // Calculate the lifetime in ticks based on the entity's lifetime in seconds.
            var lifetimeInTicks = (uint)(destroyOnTimer.ValueRW.Value * simulationTickRate);

            // Calculate the target tick at which the entity should be destroyed.
            var targetTick = currentTick;
            targetTick.Add(lifetimeInTicks);

            // Add the `DestroyAtTick` component to the entity with the calculated destruction tick.
            ecb.AddComponent(entity, new DestroyAtTick { Value = targetTick });
        }

        // Apply all queued entity modifications.
        ecb.Playback(state.EntityManager);
    }
}