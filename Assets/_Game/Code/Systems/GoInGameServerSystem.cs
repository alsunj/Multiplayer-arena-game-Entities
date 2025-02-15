using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Unity.Collections.Allocator.Temp)
            .WithAll<GoInGameRequestRpc>().WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((
                     RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest,
                     Entity entity) in
                 SystemAPI.Query
                     <RefRO<ReceiveRpcCommandRequest>>().WithAll<GoInGameRequestRpc>().WithEntityAccess())
        {
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(receiveRpcCommandRequest.ValueRO.SourceConnection);
            Debug.Log("Client Connected to Server");
            entityCommandBuffer.DestroyEntity(entity);
        }
        entityCommandBuffer.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}