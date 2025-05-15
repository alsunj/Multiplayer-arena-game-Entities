using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

/// <summary>
/// System responsible for targeting logic for NPCs. This system determines the closest valid target
/// within a specified radius for each NPC and updates the target entity accordingly.
/// </summary>
[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[UpdateBefore(typeof(ExportPhysicsWorld))]
public partial struct TargetingSystem : ISystem
{
    /// <summary>
    /// Collision filter used to define which layers the NPCs can target.
    /// </summary>
    private CollisionFilter _npcAttackFilter;

    /// <summary>
    /// Called when the system is created. Ensures the system only updates when the required components
    /// (PhysicsWorldSingleton and GamePlayingTag) are present. Initializes the collision filter for NPC targeting.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        state.RequireForUpdate<GamePlayingTag>();
        _npcAttackFilter = new CollisionFilter
        {
            BelongsTo = 1 << 6, // Target Cast
            CollidesWith = 1 << 1  // Player
        };
    }

    /// <summary>
    /// Called every frame to update the system. Schedules the NpcTargetingJob to process NPC targeting logic in parallel.
    /// </summary>
    /// <param name="state">The current state of the system.</param>
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Dependency = new NpcTargetingJob
        {
            CollisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld,
            CollisionFilter = _npcAttackFilter,
            TeamTypeLookup = SystemAPI.GetComponentLookup<TeamTypes>(true)
        }.ScheduleParallel(state.Dependency);
    }

    /// <summary>
    /// Job that processes NPC targeting logic. Identifies the closest valid target within the NPC's targeting radius
    /// and updates the target entity component.
    /// </summary>
    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct NpcTargetingJob : IJobEntity
    {
        /// <summary>
        /// The collision world used to perform overlap sphere queries for detecting potential targets.
        /// </summary>
        [ReadOnly] public CollisionWorld CollisionWorld;

        /// <summary>
        /// The collision filter defining the layers that NPCs can target.
        /// </summary>
        [ReadOnly] public CollisionFilter CollisionFilter;

        /// <summary>
        /// Read-only lookup for TeamTypes components to ensure NPCs do not target entities on the same team.
        /// </summary>
        [ReadOnly] public ComponentLookup<TeamTypes> TeamTypeLookup;

        /// <summary>
        /// Executes the job for each NPC entity. Finds the closest valid target within the targeting radius
        /// and updates the NpcTargetEntity component with the target entity.
        /// </summary>
        /// <param name="enemyEntity">The NPC entity performing the targeting.</param>
        /// <param name="targetEntity">The component storing the current target entity for the NPC.</param>
        /// <param name="transform">The local transform of the NPC entity.</param>
        /// <param name="targetRadius">The targeting radius of the NPC.</param>
        [BurstCompile]
        private void Execute(Entity enemyEntity, ref NpcTargetEntity targetEntity, in LocalTransform transform,
            in NpcTargetRadius targetRadius)
        {
            // Temporary list to store potential target hits.
            var hits = new NativeList<DistanceHit>(Allocator.TempJob);

            // Perform an overlap sphere query to find potential targets within the targeting radius.
            if (CollisionWorld.OverlapSphere(transform.Position, targetRadius.Value, ref hits, CollisionFilter))
            {
                var closestDistance = float.MaxValue;
                var closestEntity = Entity.Null;

                // Iterate through the hits to find the closest valid target.
                foreach (var hit in hits)
                {
                    if (!TeamTypeLookup.TryGetComponent(hit.Entity, out var teamTypes)) continue;
                    if (teamTypes.Value == TeamTypeLookup[enemyEntity].Value) continue;
                    if (hit.Distance < closestDistance)
                    {
                        closestDistance = hit.Distance;
                        closestEntity = hit.Entity;
                    }
                }

                // Update the target entity with the closest valid target.
                targetEntity.Value = closestEntity;
            }
            else
            {
                // If no valid targets are found, set the target entity to null.
                targetEntity.Value = Entity.Null;
            }

            // Dispose of the temporary hits list.
            hits.Dispose();
        }
    }
}