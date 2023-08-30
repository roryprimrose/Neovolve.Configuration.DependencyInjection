namespace TestHost;

internal class RootConcreteService : ConsoleUpdateService
{
    private readonly Config _config;

    public RootConcreteService(Config config)
    {
        _config = config;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        return WriteValue(_config.RootValue);
    }
}