using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct ServerStartGameSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameStartProperties>();
        state.RequireForUpdate<EntititesReferences>();
        state.RequireForUpdate<GamePlayingTag>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
    }
}