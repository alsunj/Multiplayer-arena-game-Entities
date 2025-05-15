using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

/// <summary>
/// Represents an aspect for managing enemy spawning logic, including spawn timers,
/// spawn counters, and cooldowns for different enemy types.
/// </summary>
public readonly partial struct EnemySpawnerAspect : IAspect
{
    // Reference to the enemy spawn timer component.
    private readonly RefRW<EnemySpawnTimer> _enemySpawnTimer;

    // Reference to the game start properties component.
    private readonly RefRO<GameStartProperties> _gameStartProperties;

    // Reference to the spawnable enemies counter component.
    private readonly RefRW<SpawnableEnemiesCounter> _spawnableEnemiesCounter;

    /// <summary>
    /// Gets or sets the time remaining for the next slime enemy spawn.
    /// </summary>
    public float TimeForNextSlimeSpawn
    {
        get => _enemySpawnTimer.ValueRO.SlimeSpawnTimer;
        set => _enemySpawnTimer.ValueRW.SlimeSpawnTimer = value;
    }

    /// <summary>
    /// Gets or sets the time remaining for the next rogue enemy spawn.
    /// </summary>
    public float TimeForNextRogueSpawn
    {
        get => _enemySpawnTimer.ValueRO.RogueSpawnTimer;
        set => _enemySpawnTimer.ValueRW.RogueSpawnTimer = value;
    }

    /// <summary>
    /// Gets or sets the random spawn offset used to vary spawn timings.
    /// </summary>
    public float RandomSpawnOffset
    {
        get => _enemySpawnTimer.ValueRO.RandomSpawnOffset;
        set => _enemySpawnTimer.ValueRW.RandomSpawnOffset = value;
    }

    /// <summary>
    /// Gets the total number of rogue enemies to spawn, as defined in the game start properties.
    /// </summary>
    public int RogueSpawnAmount => _gameStartProperties.ValueRO.RogueEnemyAmount;

    /// <summary>
    /// Gets the total number of slime enemies to spawn, as defined in the game start properties.
    /// </summary>
    public int SlimeSpawnAmount => _gameStartProperties.ValueRO.SlimeEnemyAmount;

    /// <summary>
    /// Gets or sets the current count of spawned rogue enemies.
    /// </summary>
    public int RogueEnemyCounter
    {
        get => _spawnableEnemiesCounter.ValueRO.RogueEnemyCounter;
        set => _spawnableEnemiesCounter.ValueRW.RogueEnemyCounter = value;
    }

    /// <summary>
    /// Gets or sets the current count of spawned slime enemies.
    /// </summary>
    public int SlimeEnemyCounter
    {
        get => _spawnableEnemiesCounter.ValueRO.SlimeEnemyCounter;
        set => _spawnableEnemiesCounter.ValueRW.SlimeEnemyCounter = value;
    }

    /// <summary>
    /// Gets the cooldown duration for spawning slime enemies.
    /// </summary>
    public float SlimeSpawnCoolDown => _enemySpawnTimer.ValueRO.SlimeSpawnCooldown;

    /// <summary>
    /// Gets the cooldown duration for spawning rogue enemies.
    /// </summary>
    public float RogueSpawnCoolDown => _enemySpawnTimer.ValueRO.RogueSpawnCooldown;

    /// <summary>
    /// Determines whether a slime enemy can spawn based on the timer and spawn limits.
    /// </summary>
    public bool CanEnemySlimeSpawn => TimeForNextSlimeSpawn <= 0 && SlimeSpawnAmount > SlimeEnemyCounter;

    /// <summary>
    /// Determines whether a rogue enemy can spawn based on the timer and spawn limits.
    /// </summary>
    public bool CanEnemyRogueSpawn => TimeForNextRogueSpawn <= 0 && RogueSpawnAmount > RogueEnemyCounter;

    /// <summary>
    /// Decrements the spawn timers and adjusts the random spawn offset over time.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update.</param>
    public void DecrementTimers(float deltaTime)
    {
        TimeForNextSlimeSpawn -= deltaTime;
        TimeForNextRogueSpawn -= deltaTime;
        if (RandomSpawnOffset > -5)
            RandomSpawnOffset -= deltaTime;
        else
            RandomSpawnOffset = 5f;
    }

    /// <summary>
    /// Increments the counter for spawned slime enemies.
    /// </summary>
    public void IncreaseSlimeCounter()
    {
        SlimeEnemyCounter++;
    }

    /// <summary>
    /// Increments the counter for spawned rogue enemies.
    /// </summary>
    public void IncreaseRogueCounter()
    {
        RogueEnemyCounter++;
    }

    /// <summary>
    /// Resets the timer for spawning rogue enemies to the cooldown duration.
    /// </summary>
    public void ResetRoqueTimer()
    {
        TimeForNextRogueSpawn = RogueSpawnCoolDown;
    }

    /// <summary>
    /// Resets the timer for spawning slime enemies to the cooldown duration.
    /// </summary>
    public void ResetSlimeTimer()
    {
        TimeForNextSlimeSpawn = SlimeSpawnCoolDown;
    }
}