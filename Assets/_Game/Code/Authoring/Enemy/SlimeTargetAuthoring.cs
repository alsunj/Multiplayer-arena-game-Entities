using Unity.Entities;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;

public class SlimeTargetAuthoring : MonoBehaviour
{
    public float NpcTargetRadius;
    public float AttackCooldownTime;

    public NetCodeConfig NetCodeConfig;

    public int SimulationTickRate => NetCodeConfig.ClientServerTickRate.SimulationTickRate;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public class SlimeTargetBaker : Baker<SlimeTargetAuthoring>
    {
        public override void Bake(SlimeTargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new NpcTargetRadius { Value = authoring.NpcTargetRadius });
            AddComponent(entity, new NpcAttackProperties
            {
                CooldownTickCount = (uint)(authoring.AttackCooldownTime * authoring.SimulationTickRate)
            });
            AddComponent<NpcTargetEntity>(entity);
            AddComponent<SlimeTag>(entity);
            AddBuffer<NpcAttackCooldown>(entity);
        }
    }
}