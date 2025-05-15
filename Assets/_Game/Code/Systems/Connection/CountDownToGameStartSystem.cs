using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial class CountdownToGameStartSystem : SystemBase
{
    /// <summary>
    /// Action invoked to update the countdown text with the number of seconds remaining until the game starts.
    /// </summary>
    public Action<int> OnUpdateCountdownText;

    /// <summary>
    /// Action invoked when the countdown ends and the game starts.
    /// </summary>
    public Action OnCountdownEnd;

    /// <summary>
    /// Called when the system is created. Ensures the system only updates when a `NetworkTime` component is present.
    /// </summary>
    protected override void OnCreate()
    {
        RequireForUpdate<NetworkTime>();
    }

    /// <summary>
    /// Called every frame to process the countdown to the game start. Updates the countdown text or triggers
    /// the game start logic when the countdown ends.
    /// </summary>
    protected override void OnUpdate()
    {
        // Retrieve the current network time.
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();

        // Skip processing if this is not the first time fully predicting the current tick.
        if (!networkTime.IsFirstTimeFullyPredictingTick) return;

        // Get the current server tick.
        var currentTick = networkTime.ServerTick;

        // Create a temporary EntityCommandBuffer to queue entity modifications.
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        // Iterate through all entities with a `GameStartTick` component and a `Simulate` tag.
        foreach (var (gameStartTick, entity) in SystemAPI.Query<GameStartTick>().WithAll<Simulate>().WithEntityAccess())
        {
            // Check if the current tick is equal to or newer than the game start tick.
            if (currentTick.Equals(gameStartTick.Value) || currentTick.IsNewerThan(gameStartTick.Value))
            {
                // Create a new entity to represent the game playing state.
                var gamePlayingEntity = ecb.CreateEntity();
                ecb.SetName(gamePlayingEntity, "GamePlayingEntity");
                ecb.AddComponent<GamePlayingTag>(gamePlayingEntity);

                // Destroy the `GameStartTick` entity as the countdown has ended.
                ecb.DestroyEntity(entity);

                // Invoke the action to signal the end of the countdown.
                OnCountdownEnd?.Invoke();
            }
            else
            {
                // Calculate the number of ticks remaining until the game starts.
                var ticksToStart = gameStartTick.Value.TickIndexForValidTick - currentTick.TickIndexForValidTick;

                // Retrieve the simulation tick rate.
                var simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;

                // Calculate the number of seconds remaining until the game starts.
                var secondsToStart = (int)math.ceil((float)ticksToStart / simulationTickRate);

                // Invoke the action to update the countdown text.
                OnUpdateCountdownText?.Invoke(secondsToStart);
            }
        }

        // Apply all queued entity modifications.
        ecb.Playback(EntityManager);
    }
}