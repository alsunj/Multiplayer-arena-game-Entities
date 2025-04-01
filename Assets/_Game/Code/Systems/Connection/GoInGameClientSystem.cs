// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.NetCode;
// using UnityEngine;
//
// [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
// partial struct GoInGameClientSystem : ISystem
// {
//     [BurstCompile]
//     public void OnCreate(ref SystemState state)
//     {
//         EntityQueryBuilder entityQueryBuilder = new EntityQueryBuilder(Allocator.Temp)
//             .WithAll<NetworkId>()
//             .WithNone<NetworkStreamInGame>();
//         state.RequireForUpdate(state.GetEntityQuery(entityQueryBuilder));
//         entityQueryBuilder.Dispose();
//     }
//
//     public void OnUpdate(ref SystemState state)
//     {
//         EntityCommandBuffer entityCommandBuffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
//         foreach ((
//                      RefRO<NetworkId> networkId,
//                      Entity entity)
//                  in SystemAPI.Query
//                      <RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
//         {
//             entityCommandBuffer.AddComponent<NetworkStreamInGame>(entity);
//
//             Entity rpcEntity = entityCommandBuffer.CreateEntity();
//             GameObject playerCameraGO = new GameObject($"Camera{networkId.ValueRO.Value}");
//             playerCameraGO.AddComponent<Camera>();
//
//             // Assign the player entity to FollowPlayer script
//             FollowPlayer followScript = playerCameraGO.AddComponent<FollowPlayer>();
//             followScript.networkId = networkId.ValueRO.Value; // Store networkId instead of dire
//
//             entityCommandBuffer.AddComponent<GoInGameRequestRpc>(rpcEntity);
//             entityCommandBuffer.AddComponent<SendRpcCommandRequest>(rpcEntity);
//         }
//
//         entityCommandBuffer.Playback(state.EntityManager);
//     }
//
//     [BurstCompile]
//     public void OnDestroy(ref SystemState state)
//     {
//     }
// }

