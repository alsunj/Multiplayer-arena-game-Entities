using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


public struct EnemySpawnPoints : IBufferElementData
{
    public float3 SpawnPoint;
}

public struct EnemySpawnTimer : IComponentData
{
    public float SlimeSpawnTimer;
    public float RogueSpawnTimer;
    public float RandomSpawnOffset;

    public float SlimeSpawnCooldown;
    public float RogueSpawnCooldown;
}