using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.NetCode;


public class FollowPlayer : MonoBehaviour
{
    public int networkId; // Store NetworkId instead of direct entity reference
    private EntityManager entityManager;
    private Entity targetEntity;
    private bool entityFound = false;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        transform.rotation = Quaternion.Euler(40, 0, 0);
    }

    void LateUpdate()
    {
        // If entity hasn't been found yet, try to find it
        if (!entityFound)
        {
            // Find the player entity that matches this camera's networkId
            var query = entityManager.CreateEntityQuery(typeof(GhostOwner), typeof(LocalTransform));
            var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
            foreach (var entity in entities)
            {
                var ghostOwner = entityManager.GetComponentData<GhostOwner>(entity);
                if (ghostOwner.NetworkId == networkId)
                {
                    targetEntity = entity;
                    entityFound = true;
                    break;
                }
            }

            entities.Dispose();
        }

        // If entity found, update camera position
        if (entityFound)
        {
            if (entityManager.Exists(targetEntity) && entityManager.HasComponent<LocalTransform>(targetEntity))
            {
                LocalTransform playerTransform = entityManager.GetComponentData<LocalTransform>(targetEntity);
                float3 playerPosition = playerTransform.Position;

                // Define smooth damp velocity
                Vector3 velocity = Vector3.zero;

                // Use SmoothDamp for a more natural movement transition
                transform.position = Vector3.SmoothDamp(transform.position, playerPosition + new float3(0, 7, -9),
                    ref velocity, 0.05f);
            }
            else
            {
                // Destroy the camera GameObject if the player entity no longer exists
                Destroy(gameObject);
            }
        }
    }
}