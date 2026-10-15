using UnityEngine;
using Zenject;

public class GameFactory : MonoBehaviour
{
    [SerializeField] private Cube _cubePrefab;
    [SerializeField] private ParticleSystem _hitParticlePrefab;

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

    public void SpawnParticleHitEffect(Vector3 position)
    {
        ParticleSystem effect = Instantiate(_hitParticlePrefab, position, Quaternion.identity);

        Canvas canvas = _game.EffectRoot.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            ParticleSystemRenderer[] renderers = effect.GetComponentsInChildren<ParticleSystemRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].sortingLayerID = canvas.sortingLayerID;
                renderers[i].sortingOrder = canvas.sortingOrder + 1;
            }
        }
    }
}
