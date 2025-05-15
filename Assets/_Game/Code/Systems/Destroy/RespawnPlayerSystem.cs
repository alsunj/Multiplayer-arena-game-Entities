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
    /// <summary>
    /// Action invoked to update the respawn countdown with the number of seconds remaining until the player respawns.
    /// </summary>
    public Action<int> OnUpdateRespawnCountdown;

    /// <summary>
    /// Action invoked when the player respawns.
    /// </summary>
    public Action OnRespawn;

    /// <summary>
    /// Called when the system is created. Ensures the system only updates when the required components
    /// and entities are present in the world.
    /// </summary>
    protected override void OnCreate()
    {
        RequireForUpdate<NetworkTime>();
        RequireForUpdate<TeamTypes>();
        RequireForUpdate<GamePlayingTag>();
    }

    /// <summary>
    /// Called when the system starts running. Instantiates the respawn entity if it does not already exist.
    /// </summary>
    protected override void OnStartRunning()
    {
        if (SystemAPI.HasSingleton<RespawnEntityTag>()) return;
        var respawnPrefab = SystemAPI.GetSingleton<EntititesReferences>().RespawnEntity;
        EntityManager.Instantiate(respawnPrefab);
    }

    /// <summary>
    /// Called every frame to process player respawn logic. Handles server-side player instantiation
    /// and client-side countdown updates.
    /// </summary>
    protected override void OnUpdate()
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        if (!networkTime.IsFirstTimeFullyPredictingTick) return;
        var currentTick = networkTime.ServerTick;

        var isServer = World.IsServer();

        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Iterate through all entities with a RespawnBufferElement buffer.
        foreach (var respawnBuffer in SystemAPI.Query<DynamicBuffer<RespawnBufferElement>>()
                     .WithAll<RespawnTickCount, Simulate>())
        {
            var respawnsToCleanup = new NativeList<int>(Allocator.Temp);

            for (var i = 0; i < respawnBuffer.Length; i++)
            {
                var curRespawn = respawnBuffer[i];

                // Check if the current tick matches or exceeds the respawn tick.
                if (currentTick.Equals(curRespawn.RespawnTick) || currentTick.IsNewerThan(curRespawn.RespawnTick))
                {
                    if (isServer)
                    {
                        // Server-side logic: Instantiate a new player entity and set its components.
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
                        // Client-side logic: Invoke the respawn action and create a camera for the new player.
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
                    // Client-side logic: Update the respawn countdown.
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

            // Remove processed respawn entries from the buffer.
            foreach (var respawnIndex in respawnsToCleanup)
            {
                respawnBuffer.RemoveAt(respawnIndex);
            }
        }

        // Apply all queued entity modifications.
        ecb.Playback(EntityManager);
    }

    /// <summary>
    /// Creates a camera for the newly respawned player.
    /// </summary>
    /// <param name="networkId">The network ID of the player.</param>
    private void CreateCameraForNewPlayer(int networkId)
    {
        GameObject playerCameraGO = new GameObject($"Camera{networkId}");
        playerCameraGO.AddComponent<Camera>();
        FollowPlayer followScript = playerCameraGO.AddComponent<FollowPlayer>();
        followScript.networkId = networkId;
    }
}