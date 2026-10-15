using UnityEngine;
using Zenject;

public class GameFactory : MonoBehaviour
{
    [SerializeField] private Cube _cubePrefab;

    private CubePool _cubePool;
    private DiContainer _container;
    private Game _game;

    [Inject]
    private void Construct(CubePool pool, DiContainer diContainer, Game gameEntry)
    {
        _cubePool = pool;
        _container = diContainer;
        _game = gameEntry;
    }

    public Cube CreateCube(CubeConfig config, RectTransform parent, bool isPaletteCube)
    {
        Cube cube = _cubePool.TryGet(out Cube cached) ? cached : _container.InstantiatePrefabForComponent<Cube>(_cubePrefab, _game.PoolRoot);
        cube.transform.SetParent(parent, false);
        cube.Configure(config, isPaletteCube);
        return cube;
    }

    public void ReleaseCube(Cube cube)
    {
        if (cube != null)
            _cubePool.Return(cube);
    }
}
