using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntititesReferences>();
        state.RequireForUpdate<NetworkId>();
        // EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Unity.Collections.Allocator.Temp)
        //     .WithAll<GoInGameRequestRpc>().WithAll<ReceiveRpcCommandRequest>();
        // state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        // entityQueryBuilder.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        EntititesReferences entititesReferences = SystemAPI.GetSingleton<EntititesReferences>();
        foreach ((
                     RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest,
                     Entity entity) in
                 SystemAPI.Query
                     <RefRO<ReceiveRpcCommandRequest>>().WithAll<GoInGameRequestRpc>().WithEntityAccess())
        {
            Entity sourceConnection =
                receiveRpcCommandRequest.ValueRO.SourceConnection; // Get the source connection entity
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(sourceConnection);
            Debug.Log("Client Connected to Server");
            entityCommandBuffer.DestroyEntity(entity);


            // Instantiate player entity and place randomly on the x axis -+10
            Entity playerEntity = entityCommandBuffer.Instantiate(entititesReferences.playerPrefabEntity);
            entityCommandBuffer.SetComponent(playerEntity, LocalTransform.FromPosition(new float3(
                UnityEngine.Random.Range(-10, +10), 0, 0)));

            NetworkId networkId = SystemAPI.GetComponent<NetworkId>(sourceConnection); // use sourceConnection
            entityCommandBuffer.AddComponent(playerEntity, new GhostOwner
            {
                NetworkId = networkId.Value
            });
            entityCommandBuffer.AddComponent(playerEntity, new NetworkEntityReference { Value = sourceConnection });

            entityCommandBuffer.AppendToBuffer(sourceConnection, new LinkedEntityGroup // use sourceConnection
            {
                Value = playerEntity
            });
        }

        entityCommandBuffer.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}