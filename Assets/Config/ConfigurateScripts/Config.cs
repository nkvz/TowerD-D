using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Tower D&D/Game Config")]
public class Config : ScriptableObject
{
    [SerializeField] private int _cubeCount = 20;
    [SerializeField] private GameTextConfig _gameTextConfig;
    [SerializeField] private List<CubeConfig> _cubes = new List<CubeConfig>();
    
    public int CubeCount => _cubeCount;
    public GameTextConfig GameTextConfig => _gameTextConfig;
    public IReadOnlyList<CubeConfig> Cubes => _cubes;

    public CubeConfig GetCube(int index) => _cubes[index % _cubes.Count];

    private void OnValidate() => _cubeCount = Mathf.Max(0, _cubeCount);
}