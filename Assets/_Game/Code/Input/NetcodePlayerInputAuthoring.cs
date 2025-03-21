using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

public class NetcodePlayerInputAuthoring : MonoBehaviour
{
    public float sprintRemaining = 5f;
    public float sprintDuration = 5f;
    public float sprintSpeed = 12f;
    public float walkSpeed = 9f;
    public float sprintCooldownReset = 2f;

    public class Baker : Baker<NetcodePlayerInputAuthoring>
    {
        public override void Bake(NetcodePlayerInputAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new NetcodePlayerInput());
            AddComponent(entity, new PlayerSprintData
            {
                isSprinting = false,
                isSprintCooldown = false,
                sprintRemaining = authoring.sprintRemaining,
                sprintDuration = authoring.sprintDuration,
                sprintSpeed = authoring.sprintSpeed,
                walkSpeed = authoring.walkSpeed,
                sprintCooldown = 0f,
                sprintCooldownReset = authoring.sprintCooldownReset,
            });


            AddComponent(entity, new PlayerAttackData());
            AddComponent(entity, new PlayerDefenceData());
            AddComponent<PlayerTag>(entity);
        }  
    }
}