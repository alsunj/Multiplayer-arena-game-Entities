using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// System responsible for spawning enemies during the game. 
/// Handles the logic for decrementing spawn timers, checking spawn conditions, 
/// and instantiating enemies at designated spawn points.
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct EnemySpawnerSystem : ISystem
{
    /// <summary>
    /// Called when the system is created. Ensures the system only updates when the required components
    /// and entities are present in the world.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnCreate(ref SystemState state)
    {
        // Require specific components and entities for the system to update.
        state.RequireForUpdate<GamePlayingTag>();
        state.RequireForUpdate<EnemySpawnPoints>();
        state.RequireForUpdate<EnemySpawnTimer>();
        state.RequireForUpdate<EntititesReferences>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
    }

    /// <summary>
    /// Called every frame to process enemy spawning logic. 
    /// Decrements spawn timers, checks if enemies can spawn, and spawns them at designated positions.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Retrieve the time elapsed since the last frame.
        var deltaTime = SystemAPI.Time.DeltaTime;

        // Iterate through all entities with the EnemySpawnerAspect.
        foreach (EnemySpawnerAspect aspect in SystemAPI.Query<EnemySpawnerAspect>())
        {
            // Decrement spawn timers for the current aspect.
            aspect.DecrementTimers(deltaTime);

            // Check if a slime enemy can spawn.
            if (aspect.CanEnemySlimeSpawn)
            {
                // Retrieve spawn points and the command buffer for entity modifications.
                Entity enemyPropertiesEntity = SystemAPI.GetSingletonEntity<EnemySpawnPoints>();
                DynamicBuffer<EnemySpawnPoints> spawnPoints =
                    SystemAPI.GetBuffer<EnemySpawnPoints>(enemyPropertiesEntity);
                var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
                var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
                Entity enemySlimeEntity = SystemAPI.GetSingleton<EntititesReferences>().SlimeEnemyEntity;

                // Calculate the spawn position for the slime enemy.
                int slimeSpawnIndex = aspect.SlimeEnemyCounter % spawnPoints.Length;
                float randomValue = aspect.RandomSpawnOffset;
                float3 spawnPosition =
                    spawnPoints[slimeSpawnIndex].SpawnPoint + new float3(randomValue, 0, -randomValue);

                // Spawn the slime enemy and update the aspect's counters and timers.
                SpawnEnemy(ecb, enemySlimeEntity, spawnPosition);
                aspect.IncreaseSlimeCounter();
                aspect.ResetSlimeTimer();
            }

            // Check if a rogue enemy can spawn.
            if (aspect.CanEnemyRogueSpawn)
            {
                // Retrieve spawn points and the command buffer for entity modifications.
                Entity enemyPropertiesEntity = SystemAPI.GetSingletonEntity<EnemySpawnPoints>();
                DynamicBuffer<EnemySpawnPoints> spawnPoints =
                    SystemAPI.GetBuffer<EnemySpawnPoints>(enemyPropertiesEntity);
                var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
                var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
                Entity enemyRogueEntity = SystemAPI.GetSingleton<EntititesReferences>().RougeEnemyEntity;

                // Calculate the spawn position for the rogue enemy.
                int rogueSpawnIndex = aspect.RogueEnemyCounter % spawnPoints.Length;
                float randomValue = aspect.RandomSpawnOffset;
                float3 spawnPosition =
                    spawnPoints[rogueSpawnIndex].SpawnPoint + new float3(randomValue, 0, -randomValue);

                // Spawn the rogue enemy and update the aspect's counters and timers.
                SpawnEnemy(ecb, enemyRogueEntity, spawnPosition);
                aspect.IncreaseRogueCounter();
                aspect.ResetRoqueTimer();
            }
        }
    }

    /// <summary>
    /// Spawns an enemy entity at the specified position.
    /// </summary>
    /// <param name="ecb">The EntityCommandBuffer used to queue entity modifications.</param>
    /// <param name="spawnableEntity">The entity prefab to spawn.</param>
    /// <param name="position">The position where the entity should be spawned.</param>
    void SpawnEnemy(EntityCommandBuffer ecb, Entity spawnableEntity, float3 position)
    {
        // Instantiate the enemy entity and set its position.
        Entity newEnemy = ecb.Instantiate(spawnableEntity);
        ecb.SetComponent(newEnemy, LocalTransform.FromPosition(position));
    }
}