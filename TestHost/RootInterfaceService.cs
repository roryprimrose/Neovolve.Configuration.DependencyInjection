namespace TestHost;

internal class RootInterfaceService : ConsoleUpdateService
{
    private readonly Config _config;

    public RootInterfaceService(Config config)
    {
        _config = config;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        return WriteValue(_config.RootValue);
    }
}