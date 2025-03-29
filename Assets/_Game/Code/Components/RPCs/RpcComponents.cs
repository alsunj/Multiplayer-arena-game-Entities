using Unity.NetCode;
using UnityEngine;

public struct GoInGameRequestRpc : IRpcCommand
{
}

public struct ClientConnectionRpc : IRpcCommand
{
}

public struct EnemyAmountRpc : IRpcCommand
{
    public int PlayerAmount;
    public int RogueEnemyAmount;
    public int SlimeEnemyAmount;
}

public struct PlayersRemainingToStart : IRpcCommand
{
    public int Value;
}

public struct GameStartTickRpc : IRpcCommand
{
    public NetworkTick Value;
}