using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class NetcodePlayerInputAuthoring : MonoBehaviour
{
    public float sprintRemaining;
    public float sprintDuration;
    public float sprintSpeed;
    public float walkSpeed;
    public float sprintCooldownReset;

    public class Baker : Baker<NetcodePlayerInputAuthoring>
    {
        public override void Bake(NetcodePlayerInputAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new NetcodePlayerInput());
            AddComponent(entity, new PlayerSprintData
            {
                sprintRemaining = authoring.sprintRemaining,
                sprintDuration = authoring.sprintDuration,
                sprintSpeed = authoring.sprintSpeed,
                walkSpeed = authoring.walkSpeed,
                sprintCooldownReset = authoring.sprintCooldownReset,
            });
            AddComponent<PlayerTag>(entity);
        }
    }
}