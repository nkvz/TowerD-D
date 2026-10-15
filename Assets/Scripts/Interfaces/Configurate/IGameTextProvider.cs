public interface IGameTextProvider
{
    GameText Get(GameMessage message);
}

public readonly struct GameText
{
    public readonly string Text;
    public readonly UnityEngine.Color Color;

    public GameText(string text, UnityEngine.Color color)
    {
        Text = text;
        Color = color;
    }
}