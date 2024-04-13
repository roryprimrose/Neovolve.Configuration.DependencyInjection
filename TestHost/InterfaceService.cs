namespace TestHost;

internal class InterfaceService : ConsoleUpdateService
{
    private readonly IThirdConfig _config;

    public InterfaceService(IThirdConfig config)
    {
        _config = config;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        return WriteValue(_config.ThirdValue + " with timeout " + _config.Timeout);
    }
}