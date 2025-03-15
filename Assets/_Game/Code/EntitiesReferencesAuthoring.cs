using Unity.Entities;
using Unity.Physics;
using UnityEngine;
using UnityEngine.Serialization;

public class EntitiesReferencesAuthoring : MonoBehaviour
{
    public GameObject playerPrefabGameObject;
    public GameObject rougeEnemyPrefabGameObject;

    public class Baker : Baker<EntitiesReferencesAuthoring>
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            Entity playerPrefabEntity = GetEntity(authoring.playerPrefabGameObject, TransformUsageFlags.Dynamic);
            Entity rougeEnemyPrefabEntity = GetEntity(authoring.rougeEnemyPrefabGameObject, TransformUsageFlags.None);
            AddComponent(entity, new EntititesReferences
            {
                PlayerPrefabEntity = playerPrefabEntity,
                RougeEnemyPrefabEntity = rougeEnemyPrefabEntity
            });
            AddComponent<PhysicsVelocity>(entity);
        }
    }
}


public struct EntititesReferences : IComponentData
{
    public Entity PlayerPrefabEntity;
    public Entity RougeEnemyPrefabEntity;
}