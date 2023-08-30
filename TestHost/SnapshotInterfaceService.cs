namespace TestHost;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal class SnapshotInterfaceService : TimerHostedService
{
    private readonly IOptionsSnapshot<IThirdConfig> _config;
    private readonly ILogger<SnapshotInterfaceService> _logger;

    public SnapshotInterfaceService(IOptionsSnapshot<IThirdConfig> config, ILogger<SnapshotInterfaceService> logger)
    {
        _config = config;
        _logger = logger;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation(_config.Value.ThirdValue);

        return Task.CompletedTask;
    }
}