using UnityEngine;

[CreateAssetMenu(fileName = "GameTextConfig", menuName = "Tower D&D/Game Text Config")]
public class GameTextConfig : ScriptableObject, IGameTextProvider
{
    [SerializeField] private string _cubePlaced = "Cube placed in the tower";
    [SerializeField] private Color _cubePlacedColor = Color.white;
    [SerializeField] private string _cubeRemoved = "Cube dropped into the hole";
    [SerializeField] private Color _cubeRemovedColor = Color.white;
    [SerializeField] private string _cubeMissed = "Cube missed the tower";
    [SerializeField] private Color _cubeMissedColor = Color.white;
    [SerializeField] private string _holeMissed = "Cube missed the hole";
    [SerializeField] private Color _holeMissedColor = Color.white;
    [SerializeField] private string _heightLimit = "The tower has reached the top";
    [SerializeField] private Color _heightLimitColor = Color.white;
    [SerializeField] private string _emptyConfig = "Add cubes to the config";
    [SerializeField] private Color _emptyConfigColor = Color.white;
    [SerializeField] private string _reloadGame = "Reload Game.";
    [SerializeField] private Color _reloadGameColor = Color.white;
    [SerializeField] private string _clearData = "Clear Data.";
    [SerializeField] private Color _clearDataColor = Color.white;

    public GameText Get(GameMessage message)
    {
        switch (message)
        {
            case GameMessage.CubePlaced: return new GameText(_cubePlaced, _cubePlacedColor);
            case GameMessage.CubeRemoved: return new GameText(_cubeRemoved, _cubeRemovedColor);
            case GameMessage.CubeMissed: return new GameText(_cubeMissed, _cubeMissedColor);
            case GameMessage.HoleMissed: return new GameText(_holeMissed, _holeMissedColor);
            case GameMessage.HeightLimit: return new GameText(_heightLimit, _heightLimitColor);
            case GameMessage.EmptyConfig: return new GameText(_emptyConfig, _emptyConfigColor);
            case GameMessage.ReloadGame: return new GameText(_reloadGame, _reloadGameColor);
            case GameMessage.ClearData: return new GameText(_clearData, _clearDataColor);
            default: return new GameText(string.Empty, Color.white);
        }
    }
}
