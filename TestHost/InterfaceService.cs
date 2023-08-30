namespace TestHost;

using Microsoft.Extensions.Logging;

internal class InterfaceService : TimerHostedService
{
    private readonly ISecondConfig _config;
    private readonly ILogger<InterfaceService> _logger;

    public InterfaceService(ISecondConfig config, ILogger<InterfaceService> logger)
    {
        _config = config;
        _logger = logger;
    }

    protected override Task DoWork(CancellationToken cancellationToken)
    {
        _logger.LogInformation(_config.SecondValue);

        return Task.CompletedTask;
    }
}