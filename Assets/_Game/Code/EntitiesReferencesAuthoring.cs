using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour
{
    public GameObject playerPrefabGameObject;

    public class Baker : Baker<EntitiesReferencesAuthoring>
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            Entity playerPrefabEntity = GetEntity(authoring.playerPrefabGameObject, TransformUsageFlags.Dynamic);

            AddComponent(entity, new EntititesReferences
            {
                playerPrefabEntity = playerPrefabEntity
            });
            AddComponent<PhysicsVelocity>(entity);
            AddComponent<PhysicsMass>(entity, PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1f));
            AddComponent<PhysicsDamping>(entity, new PhysicsDamping { Linear = 0.01f, Angular = 0.05f });
        }
    }
}

public struct EntititesReferences : IComponentData
{
    public Entity playerPrefabEntity;
}