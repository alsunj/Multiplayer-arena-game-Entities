using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[UpdateBefore(typeof(ExportPhysicsWorld))]
public partial struct NpcTargetingSystem : ISystem
{
    private CollisionFilter _npcAttackFilter;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
        _npcAttackFilter = new CollisionFilter
        {
            BelongsTo = 1 << 6, //Target Cast
            CollidesWith = 1 << 1 | 1 << 4 //Player and structures
        };
    }


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


    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct NpcTargetingJob : IJobEntity
    {
        [ReadOnly] public CollisionWorld CollisionWorld;
        [ReadOnly] public CollisionFilter CollisionFilter;
        [ReadOnly] public ComponentLookup<TeamTypes> TeamTypeLookup;

        [BurstCompile]
        private void Execute(Entity npcEntity, ref NpcTargetEntity targetEntity, in LocalTransform transform,
            in NpcTargetRadius targetRadius)
        {
            var hits = new NativeList<DistanceHit>(Allocator.TempJob);

            if (CollisionWorld.OverlapSphere(transform.Position, targetRadius.Value, ref hits, CollisionFilter))
            {
                var closestDistance = float.MaxValue;
                var closestEntity = Entity.Null;

                foreach (var hit in hits)
                {
                    if (!TeamTypeLookup.TryGetComponent(hit.Entity, out var teamTypes)) continue;
                    if (teamTypes.Value == TeamTypeLookup[npcEntity].Value) continue;
                    if (hit.Distance < closestDistance)
                    {
                        closestDistance = hit.Distance;
                        closestEntity = hit.Entity;
                    }
                }

                targetEntity.Value = closestEntity;
            }
            else
            {
                targetEntity.Value = Entity.Null;
            }

            hits.Dispose();
        }
    }
}