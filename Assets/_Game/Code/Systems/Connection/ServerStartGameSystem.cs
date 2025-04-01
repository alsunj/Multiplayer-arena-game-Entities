using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct ServerStartGameSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<GameStartProperties>();
        state.RequireForUpdate<EntititesReferences>();
        state.RequireForUpdate<GamePlayingTag>(); // var builder = new EntityQueryBuilder(Allocator.Temp).WithAll<MobaTeamRequest, ReceiveRpcCommandRequest>();
        // state.RequireForUpdate(state.GetEntityQuery(builder));
    }
}