using Unity.Entities;
using UnityEngine;

public class DamageOnCollisionAuthoring : MonoBehaviour
{
    public int DamageOnTrigger;

    public class DamageOnCollisionBaker : Baker<DamageOnCollisionAuthoring>
    {
        public override void Bake(DamageOnCollisionAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DamageOnCollision { Value = authoring.DamageOnTrigger });
            AddBuffer<AlreadyDamagedEntity>(entity);
        }
    }
}