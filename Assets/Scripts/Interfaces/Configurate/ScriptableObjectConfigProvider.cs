public class ScriptableObjectConfigProvider : IGameConfigProvider
{
    private readonly Config _config;

    public ScriptableObjectConfigProvider(Config config)
    {
        _config = config;
    }

    public Config Load() => _config;
}
