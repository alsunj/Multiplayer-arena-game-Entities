using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public readonly partial struct EnemySpawnerAspect : IAspect
{
    private readonly RefRW<EnemySpawnTimer> _enemySpawnTimer;
    private readonly RefRO<GameStartProperties> _gameStartProperties;
    private readonly RefRW<SpawnableEnemiesCounter> _spawnableEnemiesCounter;

    public float TimeForNextSlimeSpawn
    {
        get => _enemySpawnTimer.ValueRO.SlimeSpawnTimer;
        set => _enemySpawnTimer.ValueRW.SlimeSpawnTimer = value;
    }

    public float TimeForNextRogueSpawn
    {
        get => _enemySpawnTimer.ValueRO.RogueSpawnTimer;
        set => _enemySpawnTimer.ValueRW.RogueSpawnTimer = value;
    }

    public float RandomSpawnOffset
    {
        get => _enemySpawnTimer.ValueRO.RandomSpawnOffset;
        set => _enemySpawnTimer.ValueRW.RandomSpawnOffset = value;
    }

    public int RogueSpawnAmount => _gameStartProperties.ValueRO.RogueEnemyAmount;

    public int SlimeSpawnAmount => _gameStartProperties.ValueRO.SlimeEnemyAmount;

    public int RogueEnemyCounter
    {
        get => _spawnableEnemiesCounter.ValueRO.RogueEnemyCounter;
        set => _spawnableEnemiesCounter.ValueRW.RogueEnemyCounter = value;
    }

    public int SlimeEnemyCounter
    {
        get => _spawnableEnemiesCounter.ValueRO.SlimeEnemyCounter;
        set => _spawnableEnemiesCounter.ValueRW.SlimeEnemyCounter = value;
    }

    public float SlimeSpawnCoolDown => _enemySpawnTimer.ValueRO.SlimeSpawnCooldown;

    public float RogueSpawnCoolDown => _enemySpawnTimer.ValueRO.RogueSpawnCooldown;


    public bool CanEnemySlimeSpawn => TimeForNextSlimeSpawn <= 0 && SlimeSpawnAmount > SlimeEnemyCounter;
    public bool CanEnemyRogueSpawn => TimeForNextRogueSpawn <= 0 && RogueSpawnAmount > RogueEnemyCounter;


    public void DecrementTimers(float deltaTime)
    {
        TimeForNextSlimeSpawn -= deltaTime;
        TimeForNextRogueSpawn -= deltaTime;
        if (RandomSpawnOffset > -5)
            RandomSpawnOffset -= deltaTime;
        else
            RandomSpawnOffset = 5f;
    }

    public void IncreaseSlimeCounter()
    {
        SlimeEnemyCounter++;
    }

    public void IncreaseRogueCounter()
    {
        RogueEnemyCounter++;
    }

    public void ResetRoqueTimer()
    {
        TimeForNextRogueSpawn = RogueSpawnCoolDown;
    }

    public void ResetSlimeTimer()
    {
        TimeForNextSlimeSpawn = SlimeSpawnCoolDown;
    }
}