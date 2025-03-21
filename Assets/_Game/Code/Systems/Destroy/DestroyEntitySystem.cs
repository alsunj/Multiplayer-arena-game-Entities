using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
public partial struct DestroyEntitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RespawnEntityTag>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<NetworkTime>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();

        if (!networkTime.IsFirstTimeFullyPredictingTick) return;
        var currentTick = networkTime.ServerTick;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var entityManager = state.EntityManager;

        foreach (var (transform, entity) in SystemAPI.Query<RefRW<LocalTransform>>()
                     .WithAll<DestroyEntityTag, Simulate>().WithEntityAccess())
        {
            if (state.World.IsServer())
            {
                if (SystemAPI.HasComponent<PlayerTag>(entity))
                {
                    var networkEntity = SystemAPI.GetComponent<NetworkEntityReference>(entity).Value;

                    // Check if networkEntity exists BEFORE accessing its components
                    if (!entityManager.Exists(networkEntity))
                    {
                        Debug.LogWarning(
                            $"networkEntity {networkEntity.Index}, Version: {networkEntity.Version} does not exist!");
                        continue; // Skip this player entity if networkEntity is already gone
                    }

                    var respawnEntity = SystemAPI.GetSingletonEntity<RespawnEntityTag>();
                    var respawnTickCount = SystemAPI.GetComponent<RespawnTickCount>(respawnEntity).Value;

                    var respawnTick = currentTick;
                    respawnTick.Add(respawnTickCount);

                    // Store NetworkId BEFORE destroying entity or networkEntity
                    int networkIdValue = SystemAPI.GetComponent<NetworkId>(networkEntity).Value;

                    ecb.AppendToBuffer(respawnEntity, new RespawnBufferElement
                    {
                        NetworkEntity = networkEntity,
                        RespawnTick = respawnTick,
                        NetworkId = networkIdValue
                    });

                    ecb.DestroyEntity(entity);
                }
                else
                {
                    ecb.DestroyEntity(entity); // Destroy non-player entities
                }
            }
            else
            {
                transform.ValueRW.Position = new float3(1000f, 1000f, 1000f);
            }
        }
    }
}