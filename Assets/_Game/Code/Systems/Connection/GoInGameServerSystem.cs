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
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(receiveRpcCommandRequest.ValueRO.SourceConnection);
            Debug.Log("Client Connected to Server");
            entityCommandBuffer.DestroyEntity(entity);


            // Instantiate player entity and place randomly on the x axis -+10
            Entity playerEntity = entityCommandBuffer.Instantiate(entititesReferences.playerPrefabEntity);
            entityCommandBuffer.SetComponent(playerEntity, LocalTransform.FromPosition(new float3(
                UnityEngine.Random.Range(-10, +10), 0, 0)));

            // Add GhostOwner component to connecting player that sent the connection rpc
            NetworkId networkId = SystemAPI.GetComponent<NetworkId>(receiveRpcCommandRequest.ValueRO.SourceConnection);
            entityCommandBuffer.AddComponent(playerEntity, new GhostOwner
            {
                NetworkId = networkId.Value
            });

            // This destroys the player entity if the that client has disconnected
            entityCommandBuffer.AppendToBuffer(receiveRpcCommandRequest.ValueRO.SourceConnection, new LinkedEntityGroup
            {
                Value = playerEntity
            });
            Animator animator = authoring.playerPrefabGameObject.GetComponent<Animator>();
            entityCommandBuffer.AddComponent(playerEntity, new AnimatorComponent
            {
                animatorEntity = animator
            });
        }

        entityCommandBuffer.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}