namespace TestHost;

using Microsoft.Extensions.Options;

internal class SnapshotConcreteService : ConsoleUpdateService
{
    private readonly IOptionsSnapshot<ThirdConfig> _config;

    public SnapshotConcreteService(IOptionsSnapshot<ThirdConfig> config)
    {
        _config = config;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        return WriteValue(_config.Value.ThirdValue + " with timeout " + _config.Value.Timeout);
    }
}