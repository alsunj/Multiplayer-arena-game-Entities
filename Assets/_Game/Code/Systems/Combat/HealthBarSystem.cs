using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// System responsible for managing health bars in the game. This includes spawning health bars for entities,
/// updating their positions and values, and cleaning up health bars when their associated entities are destroyed.
/// Does not have a requirement for GamePlayingTag, so player health can be seen when the players are spawned and the game has not started yet
/// </summary>
[UpdateAfter(typeof(TransformSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct HealthBarSystem : ISystem
{
    /// <summary>
    /// Called when the system is created. Ensures the system only updates when the required components
    /// (EndSimulationEntityCommandBufferSystem.Singleton and UIPrefabs) are present.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<UIPrefabs>();
    }

    /// <summary>
    /// Called every frame to update the system. Handles the creation, updating, and cleanup of health bars.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    public void OnUpdate(ref SystemState state)
    {
        // Retrieve the EntityCommandBuffer for queuing entity modifications.
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        // Spawn health bars for entities that do not already have one.
        foreach (var (transform, healthBarOffset, maxHitPoints, entity) in SystemAPI
                     .Query<LocalTransform, HealthBarOffset, MaxHitPoints>().WithNone<HealthBarUIReference>()
                     .WithEntityAccess())
        {
            // Instantiate a new health bar at the entity's position plus the offset.
            var healthBarPrefab = SystemAPI.ManagedAPI.GetSingleton<UIPrefabs>().PlayerHealthUIEntity;
            var spawnPosition = transform.Position + healthBarOffset.Value;
            var newHealthBar = Object.Instantiate(healthBarPrefab, spawnPosition, Quaternion.identity);

            // Initialize the health bar with the entity's maximum hit points.
            SetHealthBar(newHealthBar, maxHitPoints.Value, maxHitPoints.Value);

            // Add a reference to the health bar in the entity's components.
            ecb.AddComponent(entity, new HealthBarUIReference { Value = newHealthBar });
        }

        // Update the position and values of existing health bars.
        foreach (var (transform, healthBarOffset, currentHitPoints, maxHitPoints, healthBarUI) in SystemAPI
                     .Query<LocalTransform, HealthBarOffset, CurrentHitPoints, MaxHitPoints, HealthBarUIReference>())
        {
            // Update the health bar's position to match the entity's position plus the offset.
            var healthBarPosition = transform.Position + healthBarOffset.Value;
            healthBarUI.Value.transform.position = healthBarPosition;

            // Update the health bar's slider values to reflect the entity's current and maximum hit points.
            SetHealthBar(healthBarUI.Value, currentHitPoints.Value, maxHitPoints.Value);
        }

        // Cleanup health bars for entities that no longer exist.
        foreach (var (healthBarUI, entity) in SystemAPI.Query<HealthBarUIReference>().WithNone<LocalTransform>()
                     .WithEntityAccess())
        {
            // Destroy the health bar GameObject and remove the reference component from the entity.
            Object.Destroy(healthBarUI.Value);
            ecb.RemoveComponent<HealthBarUIReference>(entity);
        }
    }

    /// <summary>
    /// Configures the health bar's slider to display the current and maximum hit points.
    /// </summary>
    /// <param name="healthBarCanvasObject">The GameObject representing the health bar.</param>
    /// <param name="curHitPoints">The current hit points of the entity.</param>
    /// <param name="maxHitPoints">The maximum hit points of the entity.</param>
    private void SetHealthBar(GameObject healthBarCanvasObject, int curHitPoints, int maxHitPoints)
    {
        // Retrieve the Slider component from the health bar GameObject.
        var healthBarSlider = healthBarCanvasObject.GetComponentInChildren<Slider>();

        // Set the slider's minimum, maximum, and current values.
        healthBarSlider.minValue = 0;
        healthBarSlider.maxValue = maxHitPoints;
        healthBarSlider.value = curHitPoints;
    }
}