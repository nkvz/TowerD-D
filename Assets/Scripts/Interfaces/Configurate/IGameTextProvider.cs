public interface IGameTextProvider
{
    string Get(GameMessage message);
}

public enum GameMessage
{
    CubePlaced,
    CubeRemoved,
    CubeMissed,
    HoleMissed,
    HeightLimit,
    EmptyConfig
}
