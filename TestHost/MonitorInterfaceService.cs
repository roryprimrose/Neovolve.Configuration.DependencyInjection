namespace TestHost;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal class MonitorInterfaceService : TimerHostedService
{
    private readonly IOptionsMonitor<IFirstConfig> _config;
    private readonly ILogger<MonitorInterfaceService> _logger;

    public MonitorInterfaceService(IOptionsMonitor<IFirstConfig> config, ILogger<MonitorInterfaceService> logger)
    {
        _config = config;
        _logger = logger;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation(_config.CurrentValue.FirstValue);

        return Task.CompletedTask;
    }
}