using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
public partial struct DestroyEntitySystem : ISystem
{
    /// <summary>
    /// Called when the system is created. Ensures the system only updates when the required components
    /// and entities are present in the world.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnCreate(ref SystemState state)
    {
        // Require specific components and entities for the system to update.
        state.RequireForUpdate<RespawnEntityTag>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<GamePlayingTag>();
        state.RequireForUpdate<DestroyEntityTag>();
    }

    /// <summary>
    /// Called every frame to process entities marked for destruction. Handles player-specific logic
    /// such as respawn scheduling and destroys non-player entities directly.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnUpdate(ref SystemState state)
    {
        // Retrieve the current network time.
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();

        // Skip processing if this is not the first time fully predicting the current tick.
        if (!networkTime.IsFirstTimeFullyPredictingTick) return;

        // Get the current server tick.
        var currentTick = networkTime.ServerTick;

        // Retrieve the EntityCommandBuffer for queuing entity modifications.
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // Iterate through all entities with a LocalTransform component, DestroyEntityTag, and Simulate tag.
        foreach (var (transform, entity) in SystemAPI.Query<RefRW<LocalTransform>>()
                     .WithAll<DestroyEntityTag, Simulate>().WithEntityAccess())
        {
            // Check if the current world is the server.
            if (state.World.IsServer())
            {
                // Handle player entities.
                if (SystemAPI.HasComponent<PlayerTag>(entity))
                {
                    // Retrieve the network entity and respawn-related components.
                    var networkEntity = SystemAPI.GetComponent<NetworkEntityReference>(entity).Value;
                    var respawnEntity = SystemAPI.GetSingletonEntity<RespawnEntityTag>();
                    var respawnTickCount = SystemAPI.GetComponent<RespawnTickCount>(respawnEntity).Value;

                    // Calculate the tick at which the player should respawn.
                    var respawnTick = currentTick;
                    respawnTick.Add(respawnTickCount);

                    // Store the NetworkId before destroying the entity.
                    int networkIdValue = SystemAPI.GetComponent<NetworkId>(networkEntity).Value;

                    // Append the player to the respawn buffer with the calculated respawn tick.
                    ecb.AppendToBuffer(respawnEntity, new RespawnBufferElement
                    {
                        NetworkEntity = networkEntity,
                        RespawnTick = respawnTick,
                        NetworkId = networkIdValue
                    });

                    // Destroy the player entity.
                    ecb.DestroyEntity(entity);
                }
                else
                {
                    // Destroy non-player entities directly.
                    ecb.DestroyEntity(entity);
                }
            }
        }
    }
}