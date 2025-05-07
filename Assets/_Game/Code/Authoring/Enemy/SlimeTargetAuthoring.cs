using Unity.Entities;
using UnityEngine;

public class SlimeTargetAuthoring : MonoBehaviour
{
    public float NpcTargetRadius;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public class SlimeTargetBaker : Baker<SlimeTargetAuthoring>
    {
        public override void Bake(SlimeTargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new NpcTargetRadius { Value = authoring.NpcTargetRadius });
            AddComponent<NpcTargetEntity>(entity);
            AddComponent<SlimeTag>(entity);
            AddComponent<SlimeTargetDirection>(entity);
        }
    }
}