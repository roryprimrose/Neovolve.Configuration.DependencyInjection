namespace TestHost;

internal class ConcreteService : ConsoleUpdateService
{
    private readonly ThirdConfig _config;

    public ConcreteService(ThirdConfig config)
    {
        _config = config;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        return WriteValue(_config.ThirdValue + " with timeout " + _config.Timeout);
    }
}