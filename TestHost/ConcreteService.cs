namespace TestHost;

internal class ConcreteService : ConsoleUpdateService
{
    private readonly SecondConfig _config;

    public ConcreteService(SecondConfig config)
    {
        _config = config;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        return WriteValue(_config.SecondValue);
    }
}