using UnityEngine;

[CreateAssetMenu(fileName = "GameTextConfig", menuName = "Tower D&D/Game Text Config")]
public class GameTextConfig : ScriptableObject, IGameTextProvider
{
    [SerializeField] private string _cubePlaced = "Cube placed in the tower";
    [SerializeField] private string _cubeRemoved = "Cube dropped into the hole";
    [SerializeField] private string _cubeMissed = "Cube missed the tower";
    [SerializeField] private string _holeMissed = "Cube missed the hole";
    [SerializeField] private string _heightLimit = "The tower has reached the top";
    [SerializeField] private string _emptyConfig = "Add cubes to the config";

    public string Get(GameMessage message)
    {
        switch (message)
        {
            case GameMessage.CubePlaced: return _cubePlaced;
            case GameMessage.CubeRemoved: return _cubeRemoved;
            case GameMessage.CubeMissed: return _cubeMissed;
            case GameMessage.HoleMissed: return _holeMissed;
            case GameMessage.HeightLimit: return _heightLimit;
            case GameMessage.EmptyConfig: return _emptyConfig;
            default: return string.Empty;
        }
    }
}
