using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct DestroyOnTimerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<DestroyAtTick>();
        state.RequireForUpdate<GamePlayingTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;

        foreach (var (destroyAtTick, entity) in SystemAPI.Query<RefRW<DestroyAtTick>>().WithAll<Simulate>()
                     .WithNone<DestroyEntityTag>().WithEntityAccess())
        {
            if (currentTick.Equals(destroyAtTick.ValueRW.Value) || currentTick.IsNewerThan(destroyAtTick.ValueRW.Value))
            {
                ecb.AddComponent<DestroyEntityTag>(entity);
            }
        }
    }
}