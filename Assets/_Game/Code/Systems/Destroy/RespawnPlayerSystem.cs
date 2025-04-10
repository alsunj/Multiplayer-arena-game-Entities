using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial class RespawnPlayerSystem : SystemBase
{
    public Action<int> OnUpdateRespawnCountdown;
    public Action OnRespawn;

    protected override void OnCreate()
    {
        RequireForUpdate<NetworkTime>();
        RequireForUpdate<TeamTypes>();
        RequireForUpdate<GamePlayingTag>();
    }

    protected override void OnStartRunning()
    {
        if (SystemAPI.HasSingleton<RespawnEntityTag>()) return;
        var respawnPrefab = SystemAPI.GetSingleton<EntititesReferences>().RespawnEntity;
        EntityManager.Instantiate(respawnPrefab);
    }

    protected override void OnUpdate()
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        if (!networkTime.IsFirstTimeFullyPredictingTick) return;
        var currentTick = networkTime.ServerTick;

        var isServer = World.IsServer();

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var respawnBuffer in SystemAPI.Query<DynamicBuffer<RespawnBufferElement>>()
                     .WithAll<RespawnTickCount, Simulate>())
        {
            var respawnsToCleanup = new NativeList<int>(Allocator.Temp);

            for (var i = 0; i < respawnBuffer.Length; i++)
            {
                var curRespawn = respawnBuffer[i];

                if (currentTick.Equals(curRespawn.RespawnTick) || currentTick.IsNewerThan(curRespawn.RespawnTick))
                {
                    if (isServer)
                    {
                        int networkId = SystemAPI.GetComponent<NetworkId>(curRespawn.NetworkEntity).Value;

                        Entity playerPrefab = SystemAPI.GetSingleton<EntititesReferences>().PlayerPrefabEntity;
                        Entity newPlayer = ecb.Instantiate(playerPrefab);

                        ecb.SetComponent(newPlayer, new GhostOwner { NetworkId = networkId });
                        ecb.SetComponent(newPlayer,
                            LocalTransform.FromPosition(new float3(UnityEngine.Random.Range(-10, +10), 0, 0)));
                        ecb.AppendToBuffer(curRespawn.NetworkEntity, new LinkedEntityGroup { Value = newPlayer });
                        ecb.AddComponent(newPlayer, new NetworkEntityReference { Value = curRespawn.NetworkEntity });

                        respawnsToCleanup.Add(i);
                    }
                    else
                    {
                        OnRespawn?.Invoke();
                        if (SystemAPI.TryGetSingleton<NetworkId>(out var clientNetworkId) &&
                            curRespawn.NetworkId == clientNetworkId.Value)
                        {
                            CreateCameraForNewPlayer(curRespawn.NetworkId);
                        }
                    }
                }
                else if (!isServer)
                {
                    if (SystemAPI.TryGetSingleton<NetworkId>(out var networkId))
                    {
                        if (networkId.Value == curRespawn.NetworkId)
                        {
                            var ticksToRespawn = curRespawn.RespawnTick.TickIndexForValidTick -
                                                 currentTick.TickIndexForValidTick;
                            var simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
                            var secondsToStart = (int)math.ceil((float)ticksToRespawn / simulationTickRate);
                            OnUpdateRespawnCountdown?.Invoke(secondsToStart);
                        }
                    }
                }
            }

            foreach (var respawnIndex in respawnsToCleanup)
            {
                respawnBuffer.RemoveAt(respawnIndex);
            }
        }

        ecb.Playback(EntityManager);
    }

    private void CreateCameraForNewPlayer(int networkId)
    {
        GameObject playerCameraGO = new GameObject($"Camera{networkId}");
        playerCameraGO.AddComponent<Camera>();
        FollowPlayer followScript = playerCameraGO.AddComponent<FollowPlayer>();
        followScript.networkId = networkId;
    }
}