namespace TestHost;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal class SnapshotConcreteService : TimerHostedService
{
    private readonly IOptionsSnapshot<ThirdConfig> _config;
    private readonly ILogger<SnapshotConcreteService> _logger;

    public SnapshotConcreteService(IOptionsSnapshot<ThirdConfig> config, ILogger<SnapshotConcreteService> logger)
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