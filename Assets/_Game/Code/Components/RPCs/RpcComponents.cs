using Unity.NetCode;

public struct GoInGameRequestRpc : IRpcCommand
{
}

public struct ClientConnectionRpc : IRpcCommand
{
}

public struct PlayersRemainingToStart : IRpcCommand
{
    public int Value;
}

public struct GameStartTickRpc : IRpcCommand
{
    public NetworkTick Value;
}