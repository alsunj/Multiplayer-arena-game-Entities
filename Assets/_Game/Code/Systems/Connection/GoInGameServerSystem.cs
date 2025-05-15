using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem
{
    /// <summary>
    /// Called when the system is created. Ensures the system only updates when the required components
    /// and entities are present in the world.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<GameStartProperties>();
        state.RequireForUpdate<PlayerCounter>();
        state.RequireForUpdate<EntititesReferences>();
        state.RequireForUpdate<NetworkId>();
        state.RequireForUpdate<GoInGameRequestRpc>();
    }

    /// <summary>
    /// Called every frame to process incoming game entry requests from clients. Handles player instantiation,
    /// updates the player counter, and sends RPCs to clients about the game start or remaining players.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnUpdate(ref SystemState state)
    {
        // Create a temporary command buffer to queue entity modifications.
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        // Retrieve singleton components and entities required for processing.
        EntititesReferences entititesReferences = SystemAPI.GetSingleton<EntititesReferences>();
        Entity gameStartPropertiesEntity = SystemAPI.GetSingletonEntity<GameStartProperties>();
        PlayerCounter playerCounter = SystemAPI.GetComponent<PlayerCounter>(gameStartPropertiesEntity);
        GameStartProperties gameStartProperties =
            SystemAPI.GetComponent<GameStartProperties>(gameStartPropertiesEntity);

        // Iterate through all entities with a `ReceiveRpcCommandRequest` and `GoInGameRequestRpc` component.
        foreach ((
                     RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest,
                     Entity entity) in
                 SystemAPI.Query
                     <RefRO<ReceiveRpcCommandRequest>>().WithAll<GoInGameRequestRpc>().WithEntityAccess())
        {
            // Get the source connection entity from the RPC request.
            Entity sourceConnection =
                receiveRpcCommandRequest.ValueRO.SourceConnection;

            // Mark the client as "in-game" by adding the `NetworkStreamInGame` component.
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(sourceConnection);
            Debug.Log("Client Connected to Server");

            // Destroy the RPC entity after processing.
            entityCommandBuffer.DestroyEntity(entity);

            // Instantiate a player entity and set its position randomly along the x-axis.
            Entity playerEntity = entityCommandBuffer.Instantiate(entititesReferences.PlayerPrefabEntity);
            entityCommandBuffer.SetComponent(playerEntity, LocalTransform.FromPosition(new float3(
                UnityEngine.Random.Range(-10, +10), 0, 0)));

            // Retrieve the `NetworkId` component from the source connection and assign it to the player entity.
            NetworkId networkId = SystemAPI.GetComponent<NetworkId>(sourceConnection);
            entityCommandBuffer.AddComponent(playerEntity, new GhostOwner
            {
                NetworkId = networkId.Value
            });
            entityCommandBuffer.AddComponent(playerEntity, new NetworkEntityReference { Value = sourceConnection });

            // Link the player entity to the source connection using a `LinkedEntityGroup` buffer.
            entityCommandBuffer.AppendToBuffer(sourceConnection, new LinkedEntityGroup
            {
                Value = playerEntity
            });

            // Update the player counter and calculate the number of players remaining to start the game.
            playerCounter.Value++;
            int playersRemainingToStart = gameStartProperties.PlayerAmount - playerCounter.Value;

            // Create an RPC entity to notify clients about the game start or remaining players.
            var gameStartRpc = entityCommandBuffer.CreateEntity();
            if (playersRemainingToStart <= 0 && !SystemAPI.HasSingleton<GamePlayingTag>())
            {
                // Calculate the tick at which the game should start.
                var simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
                var ticksUntilStart = (uint)(simulationTickRate * gameStartProperties.CountdownTime);
                var gameStartTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
                gameStartTick.Add(ticksUntilStart);

                // Add a `GameStartTickRpc` component to the RPC entity with the calculated start tick.
                entityCommandBuffer.AddComponent(gameStartRpc, new GameStartTickRpc
                {
                    Value = gameStartTick
                });

                // Create a server-side entity to track the game start tick.
                var gameStartEntity = entityCommandBuffer.CreateEntity();
                entityCommandBuffer.AddComponent(gameStartEntity, new GameStartTick
                {
                    Value = gameStartTick
                });
            }
            else
            {
                // Add a `PlayersRemainingToStart` component to the RPC entity with the remaining player count.
                entityCommandBuffer.AddComponent(gameStartRpc,
                    new PlayersRemainingToStart { Value = playersRemainingToStart });
            }

            // Add a `SendRpcCommandRequest` component to the RPC entity to send it to clients.
            entityCommandBuffer.AddComponent<SendRpcCommandRequest>(gameStartRpc);
        }

        // Apply all queued entity modifications.
        entityCommandBuffer.Playback(state.EntityManager);

        // Update the singleton `PlayerCounter` component with the new value.
        SystemAPI.SetSingleton(playerCounter);
    }
}