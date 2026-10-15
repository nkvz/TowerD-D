using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

// Scene installer and entry point. All scene-specific references live here.
public class Game : MonoInstaller
{
    [SerializeField] private Config _config;
    [SerializeField] private UIController _uiController;
    [SerializeField] private GameFactory _gameFactory;

    private IGameTextProvider _textProvider;

    public RectTransform PoolRoot => _uiController.PoolRoot;

    public override void InstallBindings()
    {
        Container.BindInstance(this).AsSingle();
        Container.BindInstance(_config).AsSingle();
        Container.BindInstance(_uiController).AsSingle();
        Container.Bind<IGameConfigProvider>().To<ScriptableObjectConfigProvider>().AsSingle();
        Container.Bind<IPlacementRule>().To<AnyCubePlacementRule>().AsSingle();
        Container.Bind<IGameTextProvider>().FromInstance(_config.GameTextConfig);
        Container.Bind<CubePool>().AsSingle();
        Container.BindInstance(_gameFactory).AsSingle();
        Container.Bind<DragDropService>().AsSingle();
        Container.Bind<TowerService>().AsSingle();
    }

    private void Start()
    {
        Config loadedConfig = Container.Resolve<IGameConfigProvider>().Load();

        _textProvider = Container.Resolve<IGameTextProvider>();
        _uiController.InitializeControls(OnReloadClicked, OnClearClicked);
        GameFactory factory = Container.Resolve<GameFactory>();
        TowerService tower = Container.Resolve<TowerService>();
        tower.Initialize(_uiController.TowerArea, _uiController.Hole, _uiController.HoleMask, (RectTransform)_uiController.CubeScroll.transform, factory, ShowStatus);
        Container.Resolve<DragDropService>().Initialize(_uiController.CubeScroll, _uiController.DragLayer, tower, factory);

        _uiController.ClearExistingCubes();

        if (loadedConfig.CubeCount == 0 || loadedConfig.Cubes == null || loadedConfig.Cubes.Count == 0)
        {
            ShowStatus(GameMessage.EmptyConfig);
            return;
        }

        for (int i = 0; i < loadedConfig.CubeCount; i++)
        {
            factory.CreateCube(loadedConfig.GetCube(i), _uiController.CubeContainer, true);
        }
    }

    private void ShowStatus(GameMessage message)
    {
        _uiController.SetStatus(_textProvider.Get(message));
    }

    private void OnReloadClicked()
    {
        ShowStatus(GameMessage.ReloadGame);
        _uiController.CloseTransition(ReloadScene);
    }

    private void OnClearClicked()
    {
        ShowStatus(GameMessage.ClearData);
        _uiController.CloseTransition(() =>
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            ReloadScene();
        });
    }

    private static void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
