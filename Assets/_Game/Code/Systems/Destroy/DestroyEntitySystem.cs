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
        state.RequireForUpdate<GamePlayingTag>();
        state.RequireForUpdate<DestroyEntityTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();

        if (!networkTime.IsFirstTimeFullyPredictingTick) return;
        var currentTick = networkTime.ServerTick;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (transform, entity) in SystemAPI.Query<RefRW<LocalTransform>>()
                     .WithAll<DestroyEntityTag, Simulate>().WithEntityAccess())
        {
            if (state.World.IsServer())
            {
                if (SystemAPI.HasComponent<PlayerTag>(entity))
                {
                    var networkEntity = SystemAPI.GetComponent<NetworkEntityReference>(entity).Value;


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
        }
    }
}