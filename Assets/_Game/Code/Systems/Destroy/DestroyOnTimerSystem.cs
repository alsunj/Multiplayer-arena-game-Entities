using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct DestroyOnTimerSystem : ISystem
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
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<DestroyAtTick>();
        state.RequireForUpdate<GamePlayingTag>();
    }

    /// <summary>
    /// Called every frame to process entities with a `DestroyAtTick` component. Adds a `DestroyEntityTag`
    /// to entities whose destruction tick has been reached or passed.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Retrieve the EntityCommandBuffer for queuing entity modifications.
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // Get the current server tick.
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        // Iterate through all entities with a `DestroyAtTick` component and a `Simulate` tag,
        // but without a `DestroyEntityTag`.
        foreach (var (destroyAtTick, entity) in SystemAPI.Query<RefRW<DestroyAtTick>>().WithAll<Simulate>()
                     .WithNone<DestroyEntityTag>().WithEntityAccess())
        {
            // Check if the current tick is equal to or newer than the destruction tick.
            if (currentTick.Equals(destroyAtTick.ValueRW.Value) || currentTick.IsNewerThan(destroyAtTick.ValueRW.Value))
            {
                // Add the `DestroyEntityTag` to mark the entity for destruction.
                ecb.AddComponent<DestroyEntityTag>(entity);
            }
        }
    }
}