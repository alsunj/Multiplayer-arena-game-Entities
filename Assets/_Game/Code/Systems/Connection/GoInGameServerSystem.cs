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
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<GameStartProperties>();
        state.RequireForUpdate<PlayerCounter>();
        state.RequireForUpdate<EntititesReferences>();
        state.RequireForUpdate<NetworkId>();
        state.RequireForUpdate<GoInGameRequestRpc>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);


        EntititesReferences entititesReferences = SystemAPI.GetSingleton<EntititesReferences>();
        Entity gameStartPropertiesEntity = SystemAPI.GetSingletonEntity<GameStartProperties>();
        PlayerCounter playerCounter = SystemAPI.GetComponent<PlayerCounter>(gameStartPropertiesEntity);
        GameStartProperties gameStartProperties =
            SystemAPI.GetComponent<GameStartProperties>(gameStartPropertiesEntity);
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
            Entity playerEntity = entityCommandBuffer.Instantiate(entititesReferences.PlayerPrefabEntity);
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

            playerCounter.Value++;
            int playersRemainingToStart = gameStartProperties.PlayerAmount - playerCounter.Value;
            var gameStartRpc = entityCommandBuffer.CreateEntity();
            if (playersRemainingToStart <= 0 && !SystemAPI.HasSingleton<GamePlayingTag>())
            {
                var simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
                var ticksUntilStart = (uint)(simulationTickRate * gameStartProperties.CountdownTime);
                var gameStartTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
                gameStartTick.Add(ticksUntilStart);

                // sends data to client about on what tick the game should start.
                entityCommandBuffer.AddComponent(gameStartRpc, new GameStartTickRpc
                {
                    Value = gameStartTick
                });

                //creates the entity about when the game has started on server side
                var gameStartEntity = entityCommandBuffer.CreateEntity();
                entityCommandBuffer.AddComponent(gameStartEntity, new GameStartTick
                {
                    Value = gameStartTick
                });
            }
            else
            {
                entityCommandBuffer.AddComponent(gameStartRpc,
                    new PlayersRemainingToStart { Value = playersRemainingToStart });
            }

            entityCommandBuffer.AddComponent<SendRpcCommandRequest>(gameStartRpc);
        }

        entityCommandBuffer.Playback(state.EntityManager);
        SystemAPI.SetSingleton(playerCounter);
    }
}