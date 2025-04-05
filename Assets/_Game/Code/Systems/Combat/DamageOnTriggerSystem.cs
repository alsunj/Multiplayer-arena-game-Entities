using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
public partial struct DamageOnCollisionSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var damageOnCollisionJob = new DamageOnCollisionJob
        {
            DamageOnTriggerLookup = SystemAPI.GetComponentLookup<DamageOnTrigger>(true),
            TeamLookup = SystemAPI.GetComponentLookup<TeamTypes>(true),
            AlreadyDamagedLookup = SystemAPI.GetBufferLookup<AlreadyDamagedEntity>(true),
            DamageBufferLookup = SystemAPI.GetBufferLookup<DamageBufferElement>(true),
            ECB = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged)
        };

        var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
        damageOnCollisionJob.Schedule(simulationSingleton, state.Dependency).Complete();
    }
}

public struct DamageOnCollisionJob : ICollisionEventsJob
{
    [ReadOnly] public ComponentLookup<DamageOnTrigger> DamageOnTriggerLookup;
    [ReadOnly] public ComponentLookup<TeamTypes> TeamLookup;
    public BufferLookup<AlreadyDamagedEntity> AlreadyDamagedLookup;
    public BufferLookup<DamageBufferElement> DamageBufferLookup;

    public EntityCommandBuffer ECB;

    public void Execute(CollisionEvent collisionEvent)
    {
        Entity entityA = collisionEvent.EntityA;
        Entity entityB = collisionEvent.EntityB;
        Entity damageDealingEntity = Entity.Null;
        Entity damageReceivingEntity = Entity.Null;

        if (DamageBufferLookup.HasBuffer(entityA) &&
            DamageOnTriggerLookup.HasComponent(entityB))
        {
            damageReceivingEntity = entityA;
            damageDealingEntity = entityB;
        }
        else if (DamageOnTriggerLookup.HasComponent(entityA) &&
                 DamageBufferLookup.HasBuffer(entityB))
        {
            damageDealingEntity = entityA;
            damageReceivingEntity = entityB;
        }
        else
        {
            return;
        }

        if (AlreadyDamagedLookup.HasBuffer(damageDealingEntity))
        {
            var alreadyDamagedBuffer = AlreadyDamagedLookup[damageDealingEntity];
            for (int i = 0; i < alreadyDamagedBuffer.Length; i++)
            {
                if (alreadyDamagedBuffer[i].Value == damageReceivingEntity) return;
            }
        }
        else
        {
            ECB.AddBuffer<AlreadyDamagedEntity>(damageDealingEntity);
        }

        if (TeamLookup.TryGetComponent(damageDealingEntity, out var damageDealingTeam) &&
            TeamLookup.TryGetComponent(damageReceivingEntity, out var damageReceivingTeam))
        {
            if (damageDealingTeam.Value == damageReceivingTeam.Value) return;
        }

        if (DamageOnTriggerLookup.TryGetComponent(damageDealingEntity, out var damageOnTrigger))
        {
            ECB.AddComponent<DestroyEntityTag>(damageDealingEntity);
            ECB.AppendToBuffer(damageReceivingEntity, new DamageBufferElement { Value = damageOnTrigger.Value });
            ECB.AppendToBuffer(damageDealingEntity, new AlreadyDamagedEntity { Value = damageReceivingEntity });
        }
    }
}