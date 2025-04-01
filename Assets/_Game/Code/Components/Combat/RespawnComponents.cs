using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public struct RespawnEntityTag : IComponentData
{
}

public struct RespawnBufferElement : IBufferElementData
{
    [GhostField] public NetworkTick RespawnTick;
    [GhostField] public Entity NetworkEntity;
    [GhostField] public int NetworkId;
}

public struct RespawnTickCount : IComponentData
{
    public uint Value;
}

public struct EnemySpawnPoints : IComponentData
{
    public NativeArray<float3> SpawnPoints;
}

public struct EnemySpawnTimer : IComponentData
{
    public float SpawnTimer;
}

public struct NetworkEntityReference : IComponentData
{
    public Entity Value;
}