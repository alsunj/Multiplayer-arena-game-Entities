using Unity.Entities;
using Unity.Mathematics;

public struct CameraFollow : IComponentData
{
    public Entity PlayerEntity;
    public float3 Offset;
}