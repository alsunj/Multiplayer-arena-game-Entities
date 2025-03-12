using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public class CameraFollowAuthoring : MonoBehaviour
{
    public float3 Offset = new float3(0, 2, -8);

    public class Baker : Baker<CameraFollowAuthoring>
    {
        public override void Bake(CameraFollowAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CameraFollow
            {
                Offset = authoring.Offset
            });
        }
    }
}

public struct CameraFollow : IComponentData
{
    public Entity PlayerEntity;
    public float3 Offset;
}