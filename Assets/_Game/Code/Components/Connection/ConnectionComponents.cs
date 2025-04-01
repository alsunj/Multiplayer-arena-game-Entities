using Unity.Entities;
using Unity.NetCode;

public struct GamePlayingTag : IComponentData
{
}

public struct GameStartTick : IComponentData
{
    public NetworkTick Value;
}

public struct GameStartProperties : IComponentData
{
    public int CountdownTime;
    public int PlayerAmount;
    public int RogueEnemyAmount;
    public int SlimeEnemyAmount;
}

public struct PlayerCounter : IComponentData
{
    public int Value;
}


public struct SpawnableEnemiesCounter : IComponentData
{
    public int SlimeEnemyCounter;
    public int RogueEnemyCounter;
}