using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;


/// <summary>
/// System responsible for handling game entry requests from client players.
/// This system ensures that clients are marked as "in-game" and initializes their game-related components,
/// such as creating a camera and associating it with the player.
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct ClientRequestGameEntrySystem : ISystem
{
    /// <summary>
    /// Called when the system is created. Ensures the system only updates when there are entities
    /// with a `NetworkId` component but without a `NetworkStreamInGame` component.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId>()
            .WithNone<NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    /// <summary>
    /// Called every frame to process game entry requests. Marks clients as "in-game",
    /// creates a camera for each client, and sends an RPC request to notify the server.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        // Iterate through all entities with a `NetworkId` component but without a `NetworkStreamInGame` component.
        foreach ((
                     RefRO<NetworkId> networkId,
                     Entity entity)
                 in SystemAPI.Query
                     <RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
        {
            // Add the `NetworkStreamInGame` component to mark the client as "in-game".
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(entity);

            // Create a new entity for the game entry request.
            var requestGameConnection = entityCommandBuffer.CreateEntity();

            // Create a new camera GameObject for the client and associate it with the player.
            GameObject playerCameraGO = new GameObject($"Camera{networkId.ValueRO.Value}");
            playerCameraGO.AddComponent<Camera>();
            FollowPlayer followScript = playerCameraGO.AddComponent<FollowPlayer>();
            followScript.networkId = networkId.ValueRO.Value;

            // Add components to the request entity to send an RPC to the server.
            entityCommandBuffer.AddComponent<GoInGameRequestRpc>(requestGameConnection);
            entityCommandBuffer.AddComponent<SendRpcCommandRequest>(requestGameConnection);
        }

        // Apply all queued entity modifications.
        entityCommandBuffer.Playback(state.EntityManager);
    }
}

/// <summary>
/// System responsible for handling game entry requests from thin clients.
/// This system ensures that thin clients are marked as "in-game" and sends an RPC request to notify the server.
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct ThinClientRequestGameEntrySystem : ISystem
{
    /// <summary>
    /// Called when the system is created. Ensures the system only updates when there are entities
    /// with a `NetworkId` component but without a `NetworkStreamInGame` component.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId>()
            .WithNone<NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    /// <summary>
    /// Called every frame to process game entry requests. Marks thin clients as "in-game"
    /// and sends an RPC request to notify the server.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        // Iterate through all entities with a `NetworkId` component but without a `NetworkStreamInGame` component.
        foreach ((
                     RefRO<NetworkId> networkId,
                     Entity entity)
                 in SystemAPI.Query
                     <RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
        {
            // Add the `NetworkStreamInGame` component to mark the thin client as "in-game".
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(entity);

            // Create a new entity for the game entry request.
            var requestGameConnection = entityCommandBuffer.CreateEntity();

            // Add components to the request entity to send an RPC to the server.
            entityCommandBuffer.AddComponent<GoInGameRequestRpc>(requestGameConnection);
            entityCommandBuffer.AddComponent<SendRpcCommandRequest>(requestGameConnection);
        }

        // Apply all queued entity modifications.
        entityCommandBuffer.Playback(state.EntityManager);
    }
}