using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.NetCode.Hybrid;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct ClientCameraSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkId>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ghostPresentationGameObjectSystem =
            state.World.GetExistingSystemManaged<GhostPresentationGameObjectSystem>();
        var entityManager = state.EntityManager;
        
        foreach (var (cameraFollow, entity) in SystemAPI.Query<RefRO<CameraFollow>>().WithEntityAccess())
        {
            GameObject cameraGameObject =
                ghostPresentationGameObjectSystem.GetGameObjectForEntity(entityManager, entity);
            if (cameraGameObject != null)
            {
                Camera cameraComponent = cameraGameObject.GetComponent<Camera>();
                if (cameraComponent != null)
                {
                    // Set the camera's tag to "MainCamera"
                    cameraComponent.tag = "MainCamera";
                }
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
}