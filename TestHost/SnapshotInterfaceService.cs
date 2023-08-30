namespace TestHost;

using Microsoft.Extensions.Options;

internal class SnapshotInterfaceService : ConsoleUpdateService
{
    private readonly IOptionsSnapshot<IThirdConfig> _config;

    public SnapshotInterfaceService(IOptionsSnapshot<IThirdConfig> config)
    {
        _config = config;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        return WriteValue(_config.Value.ThirdValue);
    }
}