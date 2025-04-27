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
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new EntititesReferences
            {
                PlayerPrefabEntity = GetEntity(authoring.playerPrefabGameObject, TransformUsageFlags.Dynamic),
                RougeEnemyEntity = GetEntity(authoring.RougeEnemyGameObject, TransformUsageFlags.Dynamic),
                SlimeEnemyEntity = GetEntity(authoring.SlimeEnemyGameObject, TransformUsageFlags.Dynamic),
                RespawnEntity = GetEntity(authoring.RespawnEntity, TransformUsageFlags.None)
            });
            AddComponentObject(entity, new UIPrefabs
            {
                PlayerHealthUIEntity = authoring.HealthBarPrefab
            });
        }
    }
}