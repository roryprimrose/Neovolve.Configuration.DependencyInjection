namespace TestHost;

internal class RootInterfaceService : ConsoleUpdateService
{
    private readonly IConfig _config;

    public RootInterfaceService(IConfig config)
    {
        _config = config;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        return WriteValue(_config.RootValue);
    }
}