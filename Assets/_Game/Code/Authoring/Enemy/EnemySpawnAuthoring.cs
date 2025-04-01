using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class EnemySpawnAuthoring : MonoBehaviour
{
    public Transform[] spawnPoints;
    public float spawnTimer;

    public class EnemySpawnPointsBaker : Baker<EnemySpawnAuthoring>
    {
        public override void Bake(EnemySpawnAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            var spawnPointPositions = new NativeArray<float3>(authoring.spawnPoints.Length, Allocator.Persistent);
            for (int i = 0; i < authoring.spawnPoints.Length; i++)
            {
                spawnPointPositions[i] = authoring.spawnPoints[i].position;
            }

            AddComponent(entity, new EnemySpawnPoints
            {
                SpawnPoints = spawnPointPositions
            });

            AddComponent(entity, new EnemySpawnTimer
            {
                SpawnTimer = authoring.spawnTimer
            });
        }
    }
}