using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

/// <summary>
/// System responsible for handling client-side game start logic. This system processes RPCs related to the 
/// number of players remaining to start the game and the game start tick, triggering appropriate actions 
/// and updating the game state on the client.
/// </summary>
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class ClientStartGameSystem : SystemBase
{
    /// <summary>
    /// Action invoked when the number of players remaining to start the game is updated.
    /// </summary>
    public Action<int> OnUpdatePlayersRemainingToStart;

    /// <summary>
    /// Action invoked when the game start countdown begins.
    /// </summary>
    public Action OnStartGameCountdown;

    /// <summary>
    /// Called every frame to process game start-related RPCs. Handles the destruction of RPC entities, 
    /// updates the number of players remaining to start, and initializes the game start tick on the client.
    /// </summary>
    protected override void OnUpdate()
    {
        // Create a temporary EntityCommandBuffer to queue entity modifications.
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Process RPCs for updating the number of players remaining to start the game.
        foreach (var (playersRemainingToStart, entity) in SystemAPI.Query<PlayersRemainingToStart>()
                     .WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            // Destroy the RPC entity after processing.
            ecb.DestroyEntity(entity);

            // Invoke the action to update the number of players remaining to start.
            OnUpdatePlayersRemainingToStart?.Invoke(playersRemainingToStart.Value);
        }

        // Process RPCs for the game start tick.
        foreach (var (gameStartTick, entity) in SystemAPI.Query<GameStartTickRpc>()
                     .WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
        {
            // Destroy the RPC entity after processing.
            ecb.DestroyEntity(entity);

            // Invoke the action to start the game countdown.
            OnStartGameCountdown?.Invoke();

            // Create a new entity to store the game start tick on the client side.
            var gameStartEntity = ecb.CreateEntity();

            // Add the GameStartTick component to the new entity with the received tick value.
            ecb.AddComponent(gameStartEntity, new GameStartTick
            {
                Value = gameStartTick.Value
            });
        }

        // Apply all queued entity modifications.
        ecb.Playback(EntityManager);
    }
}