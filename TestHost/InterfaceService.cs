namespace TestHost;

internal class InterfaceService : ConsoleUpdateService
{
    private readonly ISecondConfig _config;

    public InterfaceService(ISecondConfig config)
    {
        _config = config;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        return WriteValue(_config.SecondValue);
    }
}