using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour
{
    public GameObject playerPrefabGameObject;
    public GameObject RespawnEntity;
    public GameObject RougeEnemyGameObject;
    public GameObject SlimeEnemyGameObject;

    public GameObject HealthBarPrefab;

    public class Baker : Baker<EntitiesReferencesAuthoring>
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntititesReferences
            {
                PlayerPrefabEntity = GetEntity(authoring.playerPrefabGameObject, TransformUsageFlags.Dynamic),
                RougeEnemyEntity = GetEntity(authoring.RougeEnemyGameObject, TransformUsageFlags.Dynamic),
                SlimeEnemyEntity = GetEntity(authoring.SlimeEnemyGameObject, TransformUsageFlags.Dynamic),
                RespawnEntity = GetEntity(authoring.RespawnEntity, TransformUsageFlags.None)
            });
            AddComponentObject(entity, new UIPrefabs
            {
                HealthBar = authoring.HealthBarPrefab,
            });
            // AddComponent<PhysicsMass>(entity, PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1f));
            // AddComponent<PhysicsDamping>(entity, new PhysicsDamping { Linear = 0.01f, Angular = 0.05f });
        }
    }
}


public struct EntititesReferences : IComponentData
{
    public Entity PlayerPrefabEntity;
    public Entity RougeEnemyEntity;
    public Entity SlimeEnemyEntity;

    public Entity RespawnEntity;
}

public class UIPrefabs : IComponentData
{
    public GameObject HealthBar;
}