using Unity.Entities;
using UnityEngine;

public struct EntititesReferences : IComponentData
{
    public Entity PlayerPrefabEntity;
    public Entity RougeEnemyEntity;
    public Entity SlimeEnemyEntity;
    public Entity RespawnEntity;
}

public class UIPrefabs : IComponentData
{
    public GameObject PlayerHealthUIEntity;
}