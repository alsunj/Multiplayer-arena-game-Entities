using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct EnemySpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GamePlayingTag>();
        state.RequireForUpdate<EnemySpawnPoints>();
        state.RequireForUpdate<EnemySpawnTimer>();
        state.RequireForUpdate<EntititesReferences>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        Entity enemyPropertiesEntity = SystemAPI.GetSingletonEntity<EnemySpawnPoints>();
        DynamicBuffer<EnemySpawnPoints> spawnPoints = SystemAPI.GetBuffer<EnemySpawnPoints>(enemyPropertiesEntity);
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var aspect in SystemAPI.Query<EnemySpawnerAspect>())
        {
            aspect.DecrementTimers(deltaTime);
            if (aspect.CanEnemySlimeSpawn)
            {
                Entity enemySlimeEntity = SystemAPI.GetSingleton<EntititesReferences>().SlimeEnemyEntity;
                int slimeSpawnIndex = aspect.SlimeEnemyCounter % spawnPoints.Length;
                float randomValue = aspect.RandomSpawnOffset;
                float3 spawnPosition =
                    spawnPoints[slimeSpawnIndex].SpawnPoint + new float3(randomValue, 0, -randomValue);
                SpawnEnemy(ecb, enemySlimeEntity, spawnPosition);
                aspect.IncreaseSlimeCounter();
                aspect.ResetSlimeTimer();
            }

            if (aspect.CanEnemyRogueSpawn)
            {
                Entity enemyRogueEntity = SystemAPI.GetSingleton<EntititesReferences>().RougeEnemyEntity;
                int rogueSpawnIndex = aspect.RogueEnemyCounter % spawnPoints.Length;
                float randomValue = aspect.RandomSpawnOffset;
                float3 spawnPosition =
                    spawnPoints[rogueSpawnIndex].SpawnPoint + new float3(randomValue, 0, -randomValue);
                SpawnEnemy(ecb, enemyRogueEntity, spawnPosition);
                aspect.IncreaseRogueCounter();
                aspect.ResetRoqueTimer();
            }
        }
    }

    void SpawnEnemy(EntityCommandBuffer ecb, Entity spawnableEntity, float3 position)
    {
        Entity newEnemy = ecb.Instantiate(spawnableEntity);
        ecb.SetComponent(newEnemy,
            LocalTransform.FromPosition(position));
    }
}