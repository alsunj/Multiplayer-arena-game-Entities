using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class EnemySpawnAuthoring : MonoBehaviour
{
    public Transform[] spawnPoints;


    public class EnemySpawnPointsBaker : Baker<EnemySpawnAuthoring>
    {
        public override void Bake(EnemySpawnAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            DynamicBuffer<EnemySpawnPoints> dynamicBuffer = AddBuffer<EnemySpawnPoints>(entity);
            foreach (Transform spawnPoint in authoring.spawnPoints)
            {
                dynamicBuffer.Add(new EnemySpawnPoints
                {
                    SpawnPoint = new float3(spawnPoint.position)
                });
            }
        }
    }
}