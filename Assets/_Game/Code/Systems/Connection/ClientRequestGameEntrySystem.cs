using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;


[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct ClientRequestGameEntrySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId>()
            .WithNone<NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((
                     RefRO<NetworkId> networkId,
                     Entity entity)
                 in SystemAPI.Query
                     <RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
        {
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(entity);

            var requestGameConnection = entityCommandBuffer.CreateEntity();
            GameObject playerCameraGO = new GameObject($"Camera{networkId.ValueRO.Value}");
            playerCameraGO.AddComponent<Camera>();
            FollowPlayer followScript = playerCameraGO.AddComponent<FollowPlayer>();
            followScript.networkId = networkId.ValueRO.Value; // Store networkId instead of dire

            entityCommandBuffer.AddComponent<GoInGameRequestRpc>(requestGameConnection);
            entityCommandBuffer.AddComponent<SendRpcCommandRequest>(requestGameConnection);
        }

        entityCommandBuffer.Playback(state.EntityManager);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ThinClientSimulation)]
public partial struct ThinClientRequestGameEntrySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId>()
            .WithNone<NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
        entityQueryBuilder.Dispose();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach ((
                     RefRO<NetworkId> networkId,
                     Entity entity)
                 in SystemAPI.Query
                     <RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
        {
            entityCommandBuffer.AddComponent<NetworkStreamInGame>(entity);

            var requestGameConnection = entityCommandBuffer.CreateEntity();

            entityCommandBuffer.AddComponent<GoInGameRequestRpc>(requestGameConnection);
            entityCommandBuffer.AddComponent<SendRpcCommandRequest>(requestGameConnection);
        }

        entityCommandBuffer.Playback(state.EntityManager);
    }
}