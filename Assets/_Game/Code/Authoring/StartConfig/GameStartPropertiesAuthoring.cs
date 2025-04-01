using Unity.Entities;
using UnityEngine;

public class GameStartPropertiesAuthoring : MonoBehaviour
{
    public int gameStartCountDownTime;


    public class GameStartPropertiesBaker : Baker<GameStartPropertiesAuthoring>
    {
        public override void Bake(GameStartPropertiesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new GameStartProperties
            {
                CountdownTime = authoring.gameStartCountDownTime,
            });
            AddComponent(entity, new SpawnableEnemiesCounter
            {
                SlimeEnemyCounter = 0,
                RogueEnemyCounter = 0
            });
            AddComponent<PlayersRemainingToStart>(entity);
        }
    }
}